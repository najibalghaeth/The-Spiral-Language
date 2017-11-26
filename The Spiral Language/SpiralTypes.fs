﻿module Spiral.Types

// Global open
open System
open System.Collections.Generic
open HashConsing
open System.Text

// Language types
type LayoutType =
    | LayoutStack
    | LayoutPackedStack
    | LayoutHeap
    | LayoutHeapMutable

type Node<'a>(expr:'a, symbol:int) = 
    member x.Expression = expr
    member x.Symbol = symbol
    override x.ToString() = sprintf "<tag %i>" symbol
    override x.GetHashCode() = symbol
    override x.Equals(y) = 
        match y with 
        | :? Node<'a> as y -> symbol = y.Symbol
        | _ -> failwith "Invalid equality for Node."

    interface IComparable with
        member x.CompareTo(y) = 
            match y with
            | :? Node<'a> as y -> compare symbol y.Symbol
            | _ -> failwith "Invalid comparison for Node."

let inline n (x: Node<_>) = x.Expression
let (|N|) x = n x
let (|S|) (x: Node<_>) = x.Symbol

type ModuleName = string
type ModuleCode = string
type ModuleDescription = string
type Module = Module of Node<ModuleName * Module list * ModuleDescription * ModuleCode>

type PosKey = Module * int64 * int64

let h0() = HashSet(HashIdentity.Structural)
let d0() = Dictionary(HashIdentity.Structural)

let inline memoize (memo_dict: Dictionary<_,_>) f k =
    match memo_dict.TryGetValue k with
    | true, v -> v
    | false, _ -> let v = f k in memo_dict.[k] <- v; v

let nodify (dict: Dictionary<_,_>) = memoize dict (fun x -> Node(x,dict.Count))
let nodify_module = nodify <| d0()
let module_ x = nodify_module x |> Module

type Pos<'a when 'a: equality and 'a: comparison>(pos:PosKey, expr:'a) = 
    member x.Expression = expr
    member x.Pos = pos
    override x.ToString() = sprintf "%A" expr
    override x.GetHashCode() = hash expr
    override x.Equals(y) = 
        match y with 
        | :? Pos<'a> as y -> expr = y.Expression
        | x -> failwithf "Invalid equality for Pos. Got: %A" x

    interface IComparable with
        member x.CompareTo(y) = 
            match y with
            | :? Pos<'a> as y -> compare expr y.Expression
            | x -> failwithf "Invalid comparison for Pos. Got: %A" x

type PrimitiveType =
    | UInt8T
    | UInt16T
    | UInt32T
    | UInt64T
    | Int8T
    | Int16T
    | Int32T
    | Int64T
    | Float32T
    | Float64T
    | BoolT
    | StringT
    | CharT

type Value = 
    | LitUInt8 of uint8
    | LitUInt16 of uint16
    | LitUInt32 of uint32
    | LitUInt64 of uint64
    | LitInt8 of int8
    | LitInt16 of int16
    | LitInt32 of int32
    | LitInt64 of int64
    | LitFloat32 of float32
    | LitFloat64 of float
    | LitBool of bool
    | LitString of string
    | LitChar of char

type Op =
    // Extern type constructors
    | DotNetTypeCreate
    | CudaTypeCreate
    
    // Macros
    | MacroCuda
    | MacroFs

    // Closure
    | TermFunctionTypeCreate
    | ClosureIs
    | ClosureDomain
    | ClosureRange

    // Cast
    | UnsafeConvert
    | UnsafeUpcastTo
    | UnsafeDowncastTo
    | UnsafeCoerceToArrayCudaGlobal

    // Pattern matching errors
    | ErrorPatMiss
    | ErrorPatClause

    // StringOps
    | StringLength
    | StringIndex
    | StringSlice

    // Module
    | ModuleCreate
    | ModuleWith
    | ModuleWithout
    | ModuleIs
    | ModuleValues
    | ModuleHasMember
    | ModuleMap
    | ModuleFold
    | ModuleOpen

    // Caseable
    | CaseableIs
    | CaseableBoxedIs

    // Case
    | Case

    // TriOps
    | If
    | IfStatic

    // BinOps
    | Add
    | Sub
    | Mult 
    | Div 
    | Mod 
    | LTE 
    | LT 
    | EQ 
    | NEQ 
    | GT 
    | GTE 
    | And 
    | Or
    | BitwiseAnd
    | BitwiseOr
    | BitwiseXor

    | Fix
    | Apply
    | TermCast
    | JoinPointEntryMethod
    | JoinPointEntryType
    | JoinPointEntryCuda
    | StructCreate
    | ListIndex
    | ListSliceFrom
    | ListCons
    | ListLength
    | ListIs
    | TypeAnnot
    | MapGetField
    | LayoutToNone
    | LayoutToStack
    | LayoutToPackedStack
    | LayoutToHeap
    | LayoutToHeapMutable
    | TypeGet
    | TypeUnion
    | TypeSplit
    | TypeBox
    | EqType
    | SizeOf

    // Array
    | ArrayCreate
    | ReferenceCreate
    | ArrayIndex
    | MutableSet
    | ArrayLength
   
    | ShiftLeft
    | ShiftRight

    // Static unary operations
    | PrintStatic
    | ErrorNonUnit
    | ErrorType
    | TypeLitCreate
    | TypeLitCast
    | TypeLitIs
    | Dynamize
    | LitIs
    | BoxIs

    // UnOps
    | Neg
    | Log
    | Exp
    | Tanh
    | FailWith

type ArrayType =
    | ArtDotNetHeap
    | ArtDotNetReference
    | ArtCudaGlobal of Ty
    | ArtCudaShared
    | ArtCudaLocal

and FunctionCore = string * Expr

and MapType =
    | MapTypeFunction of FunctionCore // Type level function. Can also be though of as a procedural macro.
    | MapTypeRecFunction of FunctionCore * string
    | MapTypeModule

and Pattern =
    | E
    | PatVar of string
    | PatTuple of Pattern list
    | PatCons of Pattern list
    | PatTypeEq of Pattern * Expr
    | PatActive of string * Pattern
    | PatPartActive of string * Pattern
    | PatExtActive of string * Pattern
    | PatOr of Pattern list
    | PatAnd of Pattern list
    | PatClauses of (Pattern * Expr) list
    | PatTypeLit of Value
    | PatTypeLitBind of string
    | PatLit of Value
    | PatWhen of Pattern * Expr
    | PatModule of string option * PatternModule
    | PatPos of Pos<Pattern>
    | PatTypeClosure of Pattern * Pattern

and PatternModule =
    | PatMAnd of PatternModule list
    | PatMOr of PatternModule list
    | PatMXor of PatternModule list
    | PatMNot of PatternModule
    | PatMInnerModule of string * PatternModule
    | PatMName of string
    | PatMRebind of string * Pattern
    | PatMPattern of Pattern

and Expr = 
    | V of Node<string>
    | Lit of Node<Value>
    | Pattern of Node<Pattern>
    | Function of Node<FunctionCore>
    | FunctionFilt of Node<Set<string> * Node<FunctionCore>>
    | VV of Node<Expr list>
    | Op of Node<Op * Expr list>
    | ExprPos of Pos<Expr>

and Ty =
    | PrimT of PrimitiveType
    | ListT of Ty list
    | LitT of Value
    | MapT of EnvTy * MapType
    | LayoutT of LayoutType * EnvTerm * MapType
    | TermFunctionT of Ty * Ty
    | UnionT of Set<Ty>
    | RecT of JoinPointKey
    | ArrayT of ArrayType * Ty
    | DotNetTypeT of TypedExpr
    | CudaTypeT of TypedExpr

and TypedExpr =
    // Data structures
    | TyT of Ty
    | TyV of TyTag
    | TyList of TypedExpr list
    | TyMap of EnvTerm * MapType
    | TyBox of TypedExpr * Ty
    | TyLit of Value

    // Operations
    | TyLet of TyTag * TypedExpr * TypedExpr * Ty * Trace
    | TyState of TypedExpr * TypedExpr * Ty * Trace
    | TyOp of Op * TypedExpr list * Ty
    | TyJoinPoint of JoinPointKey * JoinPointType * Arguments * Ty

and JoinPointType =
    | JoinPointClosure
    | JoinPointMethod
    | JoinPointType
    | JoinPointCuda

and JoinPointState<'a,'b> =
    | JoinPointInEvaluation of 'a
    | JoinPointDone of 'b

and Tag = int
and TyTag = Tag * Ty
and EnvTy = Map<string, Ty>
and EnvTerm = 
| EnvConsed of ConsedNode<Map<string, TypedExpr>>
| Env of Map<string, TypedExpr>
| EnvUnfiltered of Map<string, TypedExpr> * Set<string>

and JoinPointKey = Node<Expr * EnvTerm>

and Arguments = TyTag list
and Renamer = Dictionary<Tag,Tag>

// This key is for functions without arguments. It is intended that the arguments be passed in through the Environment.
and JoinPointDict<'a,'b> = Dictionary<JoinPointKey, JoinPointState<'a,'b>>
// For Common Subexpression Elimination. I need it not for its own sake, but to enable other PE based optimizations.
and CSEDict = Map<TypedExpr,TypedExpr> ref

and Trace = PosKey list

and TraceNode<'a when 'a:equality and 'a:comparison>(expr:'a, trace:Trace) = 
    member x.Expression = expr
    member x.Trace = trace
    override x.ToString() = sprintf "%A" expr
    override x.GetHashCode() = hash expr
    override x.Equals(y) = 
        match y with 
        | :? TraceNode<'a> as y -> expr = y.Expression
        | _ -> failwith "Invalid equality for TraceNode."

    interface IComparable with
        member x.CompareTo(y) = 
            match y with
            | :? TraceNode<'a> as y -> compare expr y.Expression
            | _ -> failwith "Invalid comparison for TraceNode."

type TypeOrMethod =
    | TomType of Ty
    | TomJP of JoinPointType * JoinPointKey

let inline t (x: TraceNode<_>) = x.Expression
let (|T|) x = t x

type RecursiveBehavior =
    | AnnotationDive
    | AnnotationReturn

type LangEnv = {
    rbeh: RecursiveBehavior
    ltag : int ref
    seq : (TypedExpr -> TypedExpr) ref
    env : EnvTerm
    cse_env : CSEDict
    trace : Trace
    }

exception TypeError of Trace * string

type Result<'a,'b> = Succ of 'a | Fail of 'b

// Parser types
type Userstate = {
    ops : Dictionary<string, int * FParsec.Associativity>
    semicolon_line : int64
    }

type ParserExpr =
| ParserStatement of (Expr -> Expr)
| ParserExpr of Expr

// Codegen types
type CodegenEnv = {
    branch_return: string -> string
    trace: Trace
    }

type Buf = ResizeArray<ProgramNode>
and ProgramNode =
    | Statement of sep: string * code: string
    | Indent
    | Dedent

type EnvRenamer = {
    memo : Dictionary<TypedExpr,TypedExpr>
    renamer : Dictionary<Tag,Tag>
    ref_call_args : TyTag list ref
    ref_method_pars : TyTag list ref
    }

type RenamerResult = {
    renamer' : Dictionary<Tag,Tag>
    call_args : TyTag list
    method_pars : TyTag list
    }

let cuda_kernels_name = "cuda_kernels"

type AssemblyLoadType =
    | LoadType of Type
    | LoadMap of Map<string,AssemblyLoadType>

let string_to_op =
    let cases = Microsoft.FSharp.Reflection.FSharpType.GetUnionCases(typeof<Op>)
    let dict = d0()
    cases |> Array.iter (fun x ->
        dict.[x.Name] <- Microsoft.FSharp.Reflection.FSharpValue.MakeUnion(x,[||]) :?> Op
        )
    dict.TryGetValue

let c = function
| Env env -> env
| EnvUnfiltered (env,used_vars) -> Map.filter (fun k _ -> used_vars.Contains k) env
| EnvConsed env -> env.node

let (|C|) x = c x
let (|CN|) (x: ConsedNode<_>) = x.node

/// Here are the paths on my machine:
/// path_vs2017 : C:\Program Files (x86)\Microsoft Visual Studio\2017\Community
/// path_cub : C:\cub-1.7.4
/// path_cuda80 : C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v8.0
type CompilerSettings = {
    path_vs2017 : string
    path_cub : string
    path_cuda90 : string
    cuda_includes : string list
    }

/// Wraps the argument in a list if not a tuple.
let tuple_field = function 
    | TyList args -> args
    | x -> [x]

let (|TyTuple|) x = tuple_field x

let (|TypeLit|_|) = function
    | TyT (LitT x) -> Some x
    | _ -> None
let (|TypeString|_|) = function
    | TyT (LitT (LitString x)) -> Some x
    | _ -> None

let get_type_of_value = function
    | LitUInt8 _ -> PrimT UInt8T
    | LitUInt16 _ -> PrimT UInt16T
    | LitUInt32 _ -> PrimT UInt32T
    | LitUInt64 _ -> PrimT UInt64T
    | LitInt8 _ -> PrimT Int8T
    | LitInt16 _ -> PrimT Int16T
    | LitInt32 _ -> PrimT Int32T
    | LitInt64 _ -> PrimT Int64T
    | LitFloat32 _ -> PrimT Float32T
    | LitFloat64 _ -> PrimT Float64T   
    | LitBool _ -> PrimT BoolT
    | LitString _ -> PrimT StringT
    | LitChar _ -> PrimT CharT

let rec env_to_ty env = Map.map (fun _ -> get_type) env
and get_type = function
    | TyLit x -> get_type_of_value x
    | TyList l -> List.map get_type l |> ListT
    | TyMap(C l, t) -> MapT (env_to_ty l, t)

    | TyT t | TyV(_,t) | TyBox(_,t)
    | TyLet(_,_,_,t,_) | TyJoinPoint(_,_,_,t)
    | TyState(_,_,t,_) | TyOp(_,_,t) -> t

let (|TyType|) x = get_type x

let show_primt = function
    | UInt8T -> "uint8"
    | UInt16T -> "uint16"
    | UInt32T -> "uint32"
    | UInt64T -> "uint64"
    | Int8T -> "int8"
    | Int16T -> "int16"
    | Int32T -> "int32"
    | Int64T -> "int64"
    | Float32T -> "float32"
    | Float64T -> "float64"
    | BoolT -> "bool"
    | StringT -> "string"
    | CharT -> "char"

let show_value = function
    | LitUInt8 x -> sprintf "%iu8" x
    | LitUInt16 x -> sprintf "%iu16" x
    | LitUInt32 x -> sprintf "%iu32" x
    | LitUInt64 x -> sprintf "%iu64" x
    | LitInt8 x -> sprintf "%ii8" x
    | LitInt16 x -> sprintf "%ii16" x
    | LitInt32 x -> sprintf "%ii32" x
    | LitInt64 x -> sprintf "%ii64" x
    | LitFloat32 x -> sprintf "%ff32" x
    | LitFloat64 x -> sprintf "%ff64" x
    | LitBool x -> sprintf "%b" x
    | LitString x -> sprintf "%s" x
    | LitChar x -> sprintf "%c" x

let show_art = function
    | ArtDotNetHeap -> "array"
    | ArtDotNetReference -> "ref"
    | ArtCudaGlobal _ -> "array_cuda_global"
    | ArtCudaShared -> "array_cuda_shared"
    | ArtCudaLocal -> "array_cuda_local"

let show_layout_type = function
    | LayoutStack -> "layout_stack"
    | LayoutPackedStack -> "layout_packed_stack"
    | LayoutHeap -> "layout_heap"
    | LayoutHeapMutable -> "layout_heap_mutable"

let inline codegen_macro' show_typedexpr codegen print_type x = 
    let strb = StringBuilder()
    let inline append (x: string) = strb.Append x |> ignore
    let (|LS|) = function
            | TyLit (LitString x) | TypeString x -> x
            | _ -> failwithf "Iter's first three arguments must be strings."
    let er x = failwithf "Unknown argument in macro. Got: %s" (show_typedexpr x)
    let rec f = function
        | TyList [TypeString "text"; LS x] -> append x
        | TyList [TypeString "arg"; x] -> append (codegen x)
        | TyList [TypeString "args"; TyTuple l] -> append "("; List.map codegen l |> String.concat ", " |> append; append ")"
        | TyList [TypeString "type"; TyType x] -> append (print_type x)
        | TyList [TypeString "types"; TyTuple l] -> append "<"; List.map (get_type >> print_type) l |> String.concat ", " |> append; append ">" 
        | TyList [TypeString "iter"; TyList [LS begin_;LS sep;LS end_;ops]] ->
                append begin_
                match ops with
                | TyList (x :: xs) -> f x; List.iter (fun x -> append sep; f x) xs
                | TyList [] -> ()
                | x -> er x
                append end_
        | x -> er x
    match x with
    | TyList x -> List.iter f x
    | x -> er x
    strb.ToString()

let rec show_ty = function
    | PrimT x -> show_primt x
    | ListT l -> sprintf "[%s]" (List.map show_ty l |> String.concat ", ")
    | LitT v -> sprintf "type_lit (%s)" (show_value v)
    | MapT (v,MapTypeModule) -> 
        let body = 
            v |> Map.toArray 
            |> Array.map (fun (k,v) -> sprintf "%s=%s" k (show_ty v))
            |> String.concat "; "

        sprintf "{%s}" body
    | MapT (_, (MapTypeFunction _ | MapTypeRecFunction _)) -> "<function>"
    | LayoutT (layout_type,env,MapTypeModule) ->
        let body = 
            c env |> Map.toArray 
            |> Array.map (fun (k,v) -> sprintf "%s=%s" k (show_typedexpr v))
            |> String.concat "; "
        sprintf "%s {%s}" (show_layout_type layout_type) body
    | LayoutT (layout_type,_,(MapTypeFunction _ | MapTypeRecFunction _)) ->
        sprintf "%s <function>" (show_layout_type layout_type)
    | TermFunctionT (a,b) ->
        sprintf "(%s => %s)" (show_ty a) (show_ty b)
    | UnionT l ->
        let body =
            Set.toArray l
            |> Array.map show_ty
            |> String.concat ", "
        sprintf "union {%s}" body
    | RecT x -> sprintf "rec_type %i" x.Symbol
    | ArrayT (a,b) -> sprintf "%s (%s)" (show_art a) (show_ty b)
    | DotNetTypeT x -> sprintf "dotnet_type (%s)" (codegen_macro' show_typedexpr show_typedexpr show_ty x)
    | CudaTypeT x -> sprintf "cuda_type (%s)" (codegen_macro' show_typedexpr show_typedexpr show_ty x)

and show_typedexpr = function
    | TyT x -> sprintf "type (%s)" (show_ty x)
    | TyV (_,t) -> sprintf "var (%s)" (show_ty t)
    | TyList l -> 
        let body = List.map show_typedexpr l |> String.concat ", "
        sprintf "[%s]" body
    | TyMap (a,MapTypeModule) ->
        let body =
            Map.toArray (c a)
            |> Array.map (fun (a,b) -> sprintf "%s=%s" a (show_typedexpr b))
            |> String.concat "; "
        sprintf "{%s}" body
    | TyMap (_, (MapTypeFunction _ | MapTypeRecFunction _)) -> "<function>"
    | TyBox (a,b) -> sprintf "(boxed_type %s with %s)" (show_ty b) (show_typedexpr a)
    | TyLit v -> sprintf "lit %s" (show_value v)
    | _ -> failwith "Compiler error: The other typed expressions are forbidden to be printed as type errors. They should never appear in bindings."

let inline codegen_macro codegen print_type x = codegen_macro' show_typedexpr codegen print_type x
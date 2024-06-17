grammar PLC_Lab8_expr;

/** The start rule; begin parsing here. */
program: (statement)+ ;

statement: 
      ';'                                                    #EmptyStatement
    | primitiveType IDENTIFIER (',' IDENTIFIER)* ';'         #Declaration                       
    | expr ';'                                               #Expression    
    | 'read' IDENTIFIER (',' IDENTIFIER)* ';'                #ReadCLI                     
    | 'write' expr (',' expr)* ';'                           #WriteCLI  
    | '{' statement (statement)* '}'                         #CodeBlock   
    | 'if' '(' condition ')' statement ('else' statement)?   #IfStatement   
    | 'while' '(' condition ')' statement                    #WhileStatement 
    ;

condition:
      expr;

expr:
      assignment_expr                                #ExpressionAssignment
    | expr '?' expr ':' expr                         #TernaryExpression
    ;

unnary_expr:
      UN_MIN_OP expr                        #UnnaryMinus                            
    | NEG_OP expr                           #UnnaryNeg                       
    ;

assignment_expr:
    IDENTIFIER ASS_OP assignment_expr       #NestedAss   
    | assignment_leaf                       #LeafAss
    ;

assignment_leaf:
      leaf_common                           #AssTerminal          
    | or_expr                               #AssOr                      
    ;

or_expr:
      or_expr OR_OP or_leaf                 #NestedOr              
    | or_leaf                               #LeafOr                   
    ;

or_leaf:
      leaf_common                           #OrTerminal                             
    | and_expr                              #OrAnd                  
    ;

and_expr:
      and_expr AND_OP and_leaf              #NestedAnd                
    | and_leaf                              #LeafAnd                  
    ;

and_leaf:
      leaf_common                           #AndTerminal                
    | comp_expr                             #AndComp                    
    ;

comp_expr:
      comp_expr op=(EQ_OP|NOT_EQ_OP) comp_leaf       #NestedComp      
    | comp_leaf                                      #LeafComp  
    ;

comp_leaf:
      leaf_common                                    #CompTerminal      
    | rel_expr                                       #CompRel           
    ;

rel_expr:
      rel_expr op=(HIGH_OP|LOW_OP) rel_leaf          #NestedRel      
    | rel_leaf                                       #LeafRel          
    ;

rel_leaf:
      leaf_common                                    #RelTerminal     
    | add_expr                                       #RelAdd           
    ;

add_expr:
      add_expr op=(ADD_OP|SUB_OP|CONCAT_OP) add_leaf    #NestedAdd    
    | add_leaf                                          #LeafAdd        
    ;

add_leaf:
      leaf_common                                       #AddTerminal  
    | mul_expr                                          #AddMul        
    ;

mul_expr:
      mul_expr op=(MUL_OP|DIV_OP|MOD_OP) mul_leaf       #NestedMul    
    | mul_leaf                                          #LeafMul        
    ;

mul_leaf:
      leaf_common                                       #MulTerminal   
    ;          
    
leaf_common:
      leaf                                              #LeafCommon
    | '(' expr ')'                                      #CommonExpr       
    | unnary_expr                                       #CommonUnnary
    ;

leaf:
      INT                                               #Int
    | BOOL                                              #Bool
    | FLOAT                                             #Float
    | STRING                                            #String
    | IDENTIFIER                                        #Identifier
    ;

primitiveType
    : type=INT_KEYWORD
    | type=FLOAT_KEYWORD
    | type=BOOL_KEYWORD
    | type=STRING_KEYWORD
    ;

ASS_OP : '=' ;
UN_MIN_OP : '-' ;
OR_OP : '||' ;
AND_OP : '&&' ;
EQ_OP : '==' ;
NOT_EQ_OP : '!=' ;
LOW_OP : '<' ;
HIGH_OP : '>' ;
ADD_OP : '+' ;
SUB_OP : '-' ;
CONCAT_OP: '.' ;
MUL_OP : '*' ;
DIV_OP : '/' ;
MOD_OP : '%' ;
NEG_OP : '!' ;

INT_KEYWORD : 'int';
FLOAT_KEYWORD : 'float';
BOOL_KEYWORD: 'bool' ;
STRING_KEYWORD: 'string' ;

IDENTIFIER : [a-zA-Z][a-zA-Z0-9_]* ;
STRING : '"'[a-zA-Z0-9_.+/*,'@&%=(!){[\]};<>: -]*'"' ;
BOOL : 'true'|'false' ;
FLOAT : [0-9]+'.'[0-9]+ ;
INT : [0-9]+ ; 
WS : [ \t\r\n]+ -> skip ; // toss out whitespace
LINE_COMMENT : '//' ~[\r\n]* -> skip;

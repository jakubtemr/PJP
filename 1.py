import re

def apply_operator(operands, operator):
    right = operands.pop()
    left = operands.pop()
    if operator == '+':
        operands.append(left + right)
    elif operator == '-':
        operands.append(left - right)
    elif operator == '*':
        operands.append(left * right)
    elif operator == '/':
        operands.append(int(left / right)) 
    else:
        raise ValueError(f"Unknown operator: {operator}")

def evaluate_expression(expression):
    try:
        if re.search('[^\d\+\-\*\/\(\) ]', expression):
            return "ERROR"
        
        tokens = re.findall(r'\d+|\+|\-|\*|\/|\(|\)', expression.replace(' ', ''))
        
        operands = []
        operators = []
        
       
        precedence = {'+': 1, '-': 1, '*': 2, '/': 2}
        
        for token in tokens:
            if token.isdigit():  
                operands.append(int(token))
            elif token in '+-*/':  
                while (operators and operators[-1] in precedence and
                       precedence[operators[-1]] >= precedence[token]):
                    apply_operator(operands, operators.pop())
                operators.append(token)
            elif token == '(': 
                operators.append(token)
            elif token == ')': 
                while operators[-1] != '(':
                    apply_operator(operands, operators.pop())
                operators.pop()  
            else:
                return "ERROR"  

        while operators:
            apply_operator(operands, operators.pop())
        
        return operands[0] 
    except (IndexError, ZeroDivisionError, ValueError):
        return "ERROR"

def process_expressions(input_lines):
    n = int(input_lines[0])
    expressions = input_lines[1:]
    
    results = []
    for expression in expressions:
        result = evaluate_expression(expression)
        results.append(result)
    
    return results

example_input = [
    "4",
    "2 * (3+5)",
    "2 * ((3+5)",
    "15 - 2**7",
    "15 - 2*7*3"
]

example_output = process_expressions(example_input)
for result in example_output:
    print(result)
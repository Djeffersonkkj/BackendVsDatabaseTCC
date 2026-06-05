# Benchmark reproduzivel

Execucoes medidas: 10
Tempo conexao backend: 0,07 ms
Tempo conexao procedure: 0,00 ms
Estrategia: Cada execucao usa o mesmo cliente, vendedor, met pedido criado e restaura o estoque original dos produtos doo zera identity/cache/plano do SQL Server.

RegistrarPedido
Equivalencia de resultado: sim
Registros backend/procedure: 7/7
Media backend: 14,22 ms
Media procedure: 2,36 ms
Mediana backend/procedure: 12,95/2,19 ms
Min backend/procedure: 11,52/1,75 ms
Max backend/procedure: 21,27/3,29 ms
Desvio padrao backend/procedure: 3,15/0,51 ms
Diferenca percentual media: -83,40%
Processamento medio backend/procedure: 7,87/2,27 ms
Persistencia medio backend/procedure: 5,42/2,27 ms

Relatorio consolidado
Equivalencia de resultado: sim
Registros backend/procedure: 20/20
Media backend: 1.197,53 ms
Media procedure: 499,95 ms
Mediana backend/procedure: 1.125,57/470,66 ms
Min backend/procedure: 1.045,00/452,71 ms
Max backend/procedure: 1.628,91/742,40 ms
Desvio padrao backend/procedure: 185,78/82,02 ms
Diferenca percentual media: -58,25%

Relatorio vendas por periodo
Equivalencia de resultado: sim
Registros backend/procedure: 25/25
Media backend: 14,94 ms
Media procedure: 12,78 ms
Mediana backend/procedure: 14,22/13,06 ms
Min backend/procedure: 10,09/9,27 ms
Max backend/procedure: 29,00/19,57 ms
Desvio padrao backend/procedure: 5,24/3,14 ms
Diferenca percentual media: -14,42%
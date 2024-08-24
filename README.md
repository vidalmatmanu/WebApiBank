# Microservices - WebApiBank

Este projeto é um exemplo de aplicação de microserviços implementada em C# com .NET 8 e RabbitMQ. O objetivo é demonstrar a criação e a comunicação entre vários microserviços que incluem Cadastro de Clientes, Proposta de Crédito e Emissão de Cartão de Crédito, além de gerenciar erros e notificações.

## Estrutura do Projeto

O projeto é composto pelos seguintes microserviços:

1. **Cadastro de Clientes (CadastroCliente)**
   - Gerencia o cadastro de clientes e armazena informações no banco de dados.
   - Publica mensagens na fila `fila_novo_cliente` para os demais serviços processarem.

2. **Proposta de Crédito (PropostaCredito)**
   - Processa propostas de crédito para clientes.
   - Valida o `SocialSecurity` e atualiza o status do cliente.
   - Publica propostas na fila `fila_proposta_cliente` para o serviço de emissão de cartão.

3. **Emissão de Cartão de Crédito (CartaoCredito)**
   - Cria e processa cartões de crédito.
   - Atualiza o status do cliente para "Processado" ou "Erro".
   - Publica mensagens de erro na fila `fila_notificacao_erro_cartao` em caso de falhas.

4. **Notificação de Erro (NotificacaoErroConsumer)**
   - Consome mensagens das filas de erro e notifica o serviço de Cadastro de Clientes.
   - Atualiza o status do cliente para "Erro" em caso de problemas durante o processo.

## Requisitos

- .NET 8
- RabbitMQ
- SQL Server (ou outro banco de dados relacional compatível)

## Como Rodar

1. **Instale as Dependências**
   Certifique-se de que as dependências do projeto estão instaladas e atualizadas.

2. **Configuração do Banco de Dados**
   Configure a string de conexão para o banco de dados no arquivo de configuração do projeto.

3. **Executar os Microserviços**
   - Compile e execute cada microserviço individualmente ou configure-os para serem executados em conjunto.

4. **Publicar e Consumir Mensagens**
   - Utilize o RabbitMQ para enviar e receber mensagens entre os microserviços.

## Estrutura de Filas

- `fila_novo_cliente` - Fila para mensagens de criação de clientes.
- `fila_proposta_cliente` - Fila para mensagens de propostas de crédito.
- `fila_cartao_credito` - Fila para mensagens de emissão de cartões.
- `fila_notificacao_erro_cartao` - Fila para mensagens de erro de cartões.
- `fila_notificacao_erro_proposta` - Fila para mensagens de erro de propostas.

## Contribuição

Sinta-se à vontade para contribuir com melhorias e correções. Para contribuir:

1. Fork o repositório.
2. Crie uma nova branch para sua modificação (`git checkout -b minha-modificacao`).
3. Faça commit das suas alterações (`git commit -am 'Adiciona minha modificação'`).
4. Push para a branch (`git push origin minha-modificacao`).
5. Envie um Pull Request.

## Licença

Este projeto está licenciado sob a [MIT License](LICENSE).

## Contato

Para qualquer dúvida ou sugestão, entre em contato com [emanueledemattos@gmail.com].


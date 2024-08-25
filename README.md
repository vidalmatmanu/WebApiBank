# Microservices - WebApiBank

Este projeto é um exemplo de aplicação de microserviços implementada em C# com .NET 8 e RabbitMQ, utilizando SQL Server como banco relacional. O objetivo é demonstrar a criação e a comunicação entre vários microserviços que incluem Cadastro de Clientes, Proposta de Crédito e Emissão de Cartão de Crédito, além de gerenciar erros e notificações.

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

## Testes Unitários
Este projeto inclui uma suite de testes unitários para garantir a funcionalidade dos microserviços, especialmente focando no microserviço de Cadastro de Clientes. Os testes foram implementados utilizando o framework XUnit, e utilizam um banco de dados em memória configurado com o Microsoft.EntityFrameworkCore.InMemory para testar as interações com o DbContext.

  ## Testes Realizados:
  
1 - Teste de Adição e Recuperação de Cliente (CanAddAndRetrieveClient):
- Objetivo: Verificar se um cliente pode ser adicionado e posteriormente recuperado corretamente do banco de dados.
- Descrição: Um cliente é criado e adicionado ao CadastroClientesDbContext. Em seguida, o cliente é recuperado utilizando o Find, e o teste assegura que o cliente foi adicionado e recuperado com sucesso, comparando os valores esperados.

2 - Teste de Registro do DbContext (ConfigureServices_RegistersDbContext):
- Objetivo: Garantir que o CadastroClientesDbContext está sendo registrado corretamente no serviço de injeção de dependências.
- Descrição: O teste configura um Host e registra o DbContext usando as configurações fornecidas no appsettings.json. Em seguida, o teste verifica se o CadastroClientesDbContext foi registrado corretamente e se ele consegue se conectar ao banco de dados.

3 - Teste de Conexão com o Banco de Dados (CanConnectToDatabase)
- Objetivo: Verificar se é possível estabelecer uma conexão com o banco de dados SQL Server usando a string de conexão fornecida.
- Descrição:
  
Configuração do Contexto: O teste configura o DbContext (CadastroClientesDbContext) para usar um banco de dados SQL Server com a string de conexão fornecida diretamente no código. O banco de dados especificado é CadastroClientesDb, e a conexão é configurada para aceitar múltiplas consultas simultâneas e confiar no certificado do servidor.

Conexão com o Banco de Dados: Um objeto CadastroClientesDbContext é criado usando as opções configuradas. O método context.Database.CanConnect() é chamado para verificar se a conexão com o banco de dados é possível.
Validação: O teste usa Assert.True(canConnect, "Não foi possível conectar ao banco de dados."); para garantir que a conexão foi estabelecida com sucesso. Se canConnect for false, a mensagem de erro "Não foi possível conectar ao banco de dados." será exibida.

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


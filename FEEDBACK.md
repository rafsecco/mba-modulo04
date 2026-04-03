# Feedback – Avaliação Geral

## Organização do Projeto
- **Pontos positivos:**
  - A solução está organizada em `src/api-gateways`, `src/building-blocks`, `src/services` e `tests`, o que facilita localizar gateways, bibliotecas compartilhadas, APIs e testes.
  - O arquivo de solução `TelesEducacao.sln` está presente na raiz e o comando `dotnet build TelesEducacao.sln` executou com sucesso.
  - Há separação física de camadas em vários contextos (`Application`, `Data`, `Domain`, `API`), por exemplo em `src/services/TelesEducacao.Alunos.*`, `src/services/TelesEducacao.Conteudo.*` e `src/services/TelesEducacao.Auth.*`.
  - Existe projeto de teste dedicado em `tests/TelesEducacao.Catalogo.Domain.Tests`.
- **Pontos negativos:**
  - O repositório mantém um artefato de banco versionado em `src/services/TelesEducacao.Pagamentos.API/pagamentos.db`, que não deveria fazer parte do código-fonte versionado.
  - O projeto `src/services/TelesEducacao.WebApp.API` continua atuando como ponto de entrada agregado da aplicação e concorre com a proposta de múltiplas APIs independentes, gerando ambiguidade arquitetural.
  - Existe `appsettings.json` vazio na raiz do repositório, sem função operacional evidente.
  - O build sobe, mas com warnings de dependência vulnerável `NU1903` para `AutoMapper 15.0.1` em `src/services/TelesEducacao.Alunos.Application/TelesEducacao.Alunos.Application.csproj`, `src/services/TelesEducacao.Conteudo.Application/TelesEducacao.Conteudos.Application.csproj` e `src/services/TelesEducacao.WebApp.API/TelesEducacao.WebApp.API.csproj`.
  - O único projeto de teste identificado falhou em `tests/TelesEducacao.Catalogo.Domain.Tests/CursoTests.cs:30`, então a esteira de testes não está verde.

## Arquitetura de Microsserviços
- **Pontos positivos:**
  - Há projetos separados para Auth, Alunos, Conteúdo, Pagamentos e BFF: `src/services/TelesEducacao.Auth.Api`, `src/services/TelesEducacao.Alunos.API`, `src/services/TelesEducacao.Conteudo.API`, `src/services/TelesEducacao.Pagamentos.API` e `src/api-gateways/TelesEducacao.Bff.Plataforma`.
  - Cada contexto principal possui seu próprio `DbContext`: `AuthDbContext`, `AlunosContext`, `ConteudosContext` e `PagamentosContext`.
  - Há pastas de migração separadas por contexto em `src/services/*/Migrations`.
- **Pontos negativos:**
  - A independência entre serviços não está consistente. `src/services/TelesEducacao.Alunos.API/TelesEducacao.Alunos.API.csproj:23-30` referencia diretamente projetos de outros contextos, incluindo `TelesEducacao.Conteudos.Application`, o que cria acoplamento indevido entre bounded contexts.
  - `src/services/TelesEducacao.WebApp.API/TelesEducacao.WebApp.API.csproj:20-25` e `src/services/TelesEducacao.WebApp.API/Extensions/DependencyInjection.cs:33-65` reúnem dependências e handlers de Alunos, Conteúdo e Pagamentos no mesmo processo, caracterizando uma composição monolítica paralela ao desenho de microsserviços.
  - `src/services/TelesEducacao.WebApp.API/Program.cs:26-33` registra `ConteudosContext`, `AlunosContext` e `PagamentosContext` com a mesma connection string (`Database=TelesEducacaoDb` em `src/services/TelesEducacao.WebApp.API/appsettings.json:2-17`), o que elimina o isolamento de banco quando essa API é utilizada.
  - O fluxo de eventos entre Alunos e Pagamentos não está desacoplado por infraestrutura de mensageria entre serviços; na prática, a maior parte dos eventos passa por `IMediator` em memória (`src/building-blocks/TelesEducacao.Core/Communication/Mediator/MediatorHandler.cs:16-28`).

## Bounded Contexts e APIs
- **Pontos positivos:**
  - A Auth API concentra cadastro e login em `src/services/TelesEducacao.Auth.Api/Controllers/AuthController.cs:23-157`.
  - A Conteúdo API expõe operações de cursos e aulas em `src/services/TelesEducacao.Conteudo.API/Controllers/ConteudoController.cs:32-175`.
  - A Alunos API possui endpoints de matrícula, aulas concluídas e certificado em `src/services/TelesEducacao.Alunos.API/Controllers/AlunosController.cs:53-126`.
  - A Pagamentos API possui endpoints de pagamento e consulta de status em `src/services/TelesEducacao.Pagamentos.API/Controllers/PagamentosController.cs:58-144`.
  - O BFF existe como projeto separado em `src/api-gateways/TelesEducacao.Bff.Plataforma`.
- **Pontos negativos:**
  - O BFF está incompleto para o escopo do módulo: em `src/api-gateways/TelesEducacao.Bff.Plataforma/Controllers` existe apenas `AlunosController.cs`, sem orquestração para Auth, Conteúdo ou Pagamentos.
  - A responsabilidade de autenticação não ficou exclusiva na Auth API. `src/services/TelesEducacao.WebApp.API/Controllers/UserController.cs:29-66` e `src/services/TelesEducacao.WebApp.API/AccessControl/UserService.cs:18-47` implementam login e geração de JWT fora da Auth API.
  - `src/services/TelesEducacao.WebApp.API` mantém funcionalidades de cursos, usuários e pagamentos no mesmo serviço, contrariando a separação por contexto.
  - A Conteúdo API usa MassTransit para enviar requisições para os próprios consumers do mesmo serviço (`ConteudoController.cs:47-60` e `:77-86` junto com `Services/ConteudoIntegrationHandler.cs:18-42` e `Services/AulaIntegrationHandler.cs:18-38`). Isso adiciona complexidade, mas não resolve integração real entre bounded contexts.

## Fluxos de Negócio e Integração
- **Pontos positivos:**
  - **Login e emissão de JWT:** implementados na Auth API via `AuthController.Acessar` e `JwtService.GenerateTokenAsync` (`src/services/TelesEducacao.Auth.Api/Controllers/AuthController.cs:63-123`, `src/services/TelesEducacao.Auth.Application/Services/JwtService.cs:33-44`).
  - **Cadastro de curso e aula:** existem endpoints administrativos na Conteúdo API (`src/services/TelesEducacao.Conteudo.API/Controllers/ConteudoController.cs:32-92`).
  - **Consulta de status de pagamento:** existe endpoint dedicado em `src/services/TelesEducacao.Pagamentos.API/Controllers/PagamentosController.cs:113-144`.
- **Pontos negativos:**
  - **Matrícula e pagamento assíncronos não estão implementados de forma distribuída entre APIs.** A matrícula publica `MatriculaAdicionadaEvent` internamente em `src/services/TelesEducacao.Alunos.Application/Commands/AdicionarMatriculaCommandHandler.cs:30-41`, e o processamento no contexto de pagamentos ocorre por handler MediatR (`src/services/TelesEducacao.Pagamentos.Business/Events/PagamentoEventHandler.cs:16-30`), sem evidência de publicação/consumo via RabbitMQ entre Alunos API e Pagamentos API.
  - **Ativação/cancelamento de matrícula após pagamento não está corretamente conectada na Alunos API.** `src/services/TelesEducacao.Alunos.API/Program.cs:29-41` registra apenas dois handlers de comando e não registra `AtivarMatriculaCommandHandler`, `CancelarMatriculaCommandHandler`, `ConcluirAulaCommandHandler`, `ConcluirCursoCommandHandler` nem `MatriculaEventHandler` da camada de aplicação.
  - **Realização de aula via integração HTTP não foi implementada conforme o escopo.** Não há `HttpClient` nem integração da Conteúdo API chamando a Alunos API para registrar progresso; a busca no código da `TelesEducacao.Conteudo.API` não mostra esse fluxo.
  - **O fluxo de conclusão de aula está logicamente inconsistente.** `src/services/TelesEducacao.Alunos.Application/Commands/ConluirAulaCommand.cs:12-16` atribui o primeiro parâmetro à propriedade `MatriculaId`; o controller chama `new ConluirAulaCommand(id, aulaId)` em `src/services/TelesEducacao.Alunos.API/Controllers/AlunosController.cs:102-104`, de modo que o `id` da rota é tratado como matrícula, não como aluno.
  - **A finalização de curso não valida progresso antes de gerar certificado.** Em `src/services/TelesEducacao.Alunos.API/Controllers/AlunosController.cs:110-121`, o endpoint apenas busca a matrícula e delega o comando; em `src/services/TelesEducacao.Alunos.Application/Commands/ConcluirCursoCommand.cs:26-31`, a implementação só altera status e cria certificado.
  - **A rota de certificado está semanticamente confusa.** O endpoint `POST {id}/Matricula/Certificados` em `src/services/TelesEducacao.Alunos.API/Controllers/AlunosController.cs:110-121` usa `id` para buscar matrícula por ID, embora o prefixo do controller sugira identificação por aluno.
  - **O BFF não orquestra o valor real do curso na matrícula.** Em `src/api-gateways/TelesEducacao.Bff.Plataforma/Controllers/AlunosController.cs:118-132`, o valor é fixado em `100`, sem consulta ao contexto de conteúdo.

## Autenticação e Autorização
- **Pontos positivos:**
  - A Auth API usa ASP.NET Core Identity com `UserManager`, `SignInManager` e `RoleManager` (`src/services/TelesEducacao.Auth.Api/Program.cs:19-31`, `src/services/TelesEducacao.Auth.Application/Services/AuthService.cs:9-20`).
  - O JWT emitido pela Auth API inclui `NameIdentifier`, `Email`, `Name` e roles (`src/services/TelesEducacao.Auth.Application/Services/JwtService.cs:142-167`).
  - Há seed de roles `Admin` e `Aluno`, além de usuário administrador inicial, em `src/services/TelesEducacao.Auth.Infrastructure/Configuration/DbMigrationHelpers.cs:26-29` e `:57-75`.
- **Pontos negativos:**
  - A estratégia de validação de token não é consistente entre os serviços:
    - Alunos API e BFF usam JWKS por `AutenticacaoJwksUrl` em `src/building-blocks/TelesEducacao.WebAPI.Core/Identidade/JwtConfig.cs:11-29` e `src/services/TelesEducacao.Alunos.API/appsettings.json:12-14` / `src/api-gateways/TelesEducacao.Bff.Plataforma/appsettings.Development.json:13-15`.
    - A Auth API não expõe endpoint JWKS no código analisado.
    - Pagamentos API usa outra configuração JWT (`src/services/TelesEducacao.Pagamentos.API/Program.cs:47-78`, `src/services/TelesEducacao.Pagamentos.API/appsettings.json:12-17`) com `Issuer`, `Audience` e `Secret` diferentes da Auth API.
  - A Conteúdo API possui `[Authorize(Roles = "Admin")]` em `src/services/TelesEducacao.Conteudo.API/Controllers/ConteudoController.cs:14-17`, mas não configura autenticação JWT no startup; `src/services/TelesEducacao.Conteudo.API/Configurations/ApiConfig.cs:10-38` não chama `AddAuthentication`, `AddJwtConfiguration` nem `AddAuthorization`, e `UseApiCoreConfigurations` só executa `UseAuthorization` (`:41-51`).
  - A WebApp API também duplica autenticação com outra chave/issuer (`src/services/TelesEducacao.WebApp.API/Program.cs:55-110`) e ainda não chama `app.UseAuthentication()` no pipeline (`src/services/TelesEducacao.WebApp.API/Program.cs:118-128`).
  - A geração de token da WebApp API é incorreta para login anônimo: `src/services/TelesEducacao.WebApp.API/Controllers/UserController.cs:45-63` usa `User.Claims` da requisição atual, não as claims do usuário autenticado.
  - Há endpoints sensíveis excessivamente abertos, por exemplo `ObterPorId` e `ObterTodos` de alunos com `[AllowAnonymous]` em `src/services/TelesEducacao.Alunos.API/Controllers/AlunosController.cs:31-50` e status de pagamento anônimo em `src/services/TelesEducacao.Pagamentos.API/Controllers/PagamentosController.cs:113-144`.

## Resiliência e Comunicação
- **Pontos positivos:**
  - O BFF aplica retry e circuit breaker com Polly no `HttpClient` de alunos (`src/api-gateways/TelesEducacao.Bff.Plataforma/Program.cs:31-36`).
  - Existe uma extensão compartilhada de retry em `src/building-blocks/TelesEducacao.WebAPI.Core/Extensions/PollyExtensions.cs:9-20`.
  - Há infraestrutura de message bus com MassTransit/RabbitMQ em `src/building-blocks/TelesEducacao.MessageBus/DependencyInjectionExtensions.cs:10-29`.
- **Pontos negativos:**
  - A infraestrutura de mensageria está efetivamente conectada apenas na Conteúdo API (`src/services/TelesEducacao.Conteudo.API/Configurations/MessageBusConfig.cs:8-10` e `appsettings.json:12-18`). Não há evidência equivalente na Alunos API ou Pagamentos API para o fluxo crítico de matrícula/pagamento.
  - O fluxo mais sensível do domínio continua acoplado ao processo local por `IMediator`, em vez de publicar/consumir eventos entre serviços.
  - Não encontrei políticas de retry/circuit breaker nas integrações entre os serviços de domínio principais; a resiliência aparece apenas no BFF.
  - Não há evidência de estratégias complementares para entrega confiável de eventos críticos, como outbox, idempotência ou tratamento explícito de duplicidade/falha na confirmação de pagamento.

## Execução Local e Infraestrutura
- **Pontos positivos:**
  - Existem helpers de migração para Auth, Alunos, Conteúdo e Pagamentos (`DbMigrationHelpers.cs` em cada contexto).
  - Auth, Alunos e Conteúdo chamam seus helpers no startup (`src/services/TelesEducacao.Auth.Api/Program.cs:95-98`, `src/services/TelesEducacao.Alunos.API/Program.cs:80-82`, `src/services/TelesEducacao.Conteudo.API/Program.cs:8-11`).
  - Há `docker/docker-compose.yml` com RabbitMQ para suporte à mensageria local.
  - Swagger está configurado nas APIs principais e no BFF.
- **Pontos negativos:**
  - A Pagamentos API não executa migração automática no próprio startup; `src/services/TelesEducacao.Pagamentos.API/Program.cs:119-138` não chama `UseDbMigrationPagamentosHelper()`, apesar do helper existir em `src/services/TelesEducacao.Pagamentos.Data/Configuration/DbMigrationHelpers.cs:6-23`.
  - O seed real está implementado apenas na Auth API. Em Alunos, o helper só migra o banco e deixa o seed comentado (`src/services/TelesEducacao.Alunos.Data/Configuration/DbMigrationHelpers.cs:22-30`), o que prejudica rodar os fluxos do aluno com pouca configuração.
  - Não há evidência de uso configurado de SQLite para execução simplificada; o código usa `UseSqlServer` em todos os serviços analisados. O arquivo `pagamentos.db` aparece apenas como artefato versionado, não como estratégia formal de infraestrutura.
  - O BFF depende de `appsettings.Development.json` para URLs dos serviços (`src/api-gateways/TelesEducacao.Bff.Plataforma/appsettings.Development.json:8-15`), enquanto `appsettings.json` está praticamente vazio.
  - Como parte da operação local, o README ainda orienta executar apenas a `WebApp.API`, não o ecossistema distribuído.

## Documentação
- **Pontos positivos:**
  - O `README.md` descreve tecnologias, estrutura geral e objetivos da solução.
  - As APIs possuem Swagger configurado, o que ajuda na exploração dos endpoints quando o serviço sobe.
  - O repositório já prevê `FEEDBACK.md` na raiz para consolidação do parecer.
- **Pontos negativos:**
  - O README está desalinhado da implementação atual:
    - referencia o repositório/módulo anterior em `README.md:73-75` (`mba-modulo03`);
    - orienta execução apenas de `src/TelesEducacao.WebApp.API` em `README.md:88-92`, embora existam múltiplos serviços e um BFF;
    - não documenta subida coordenada de Auth, Conteúdo, Alunos, Pagamentos e BFF.
  - O README não documenta o papel do RabbitMQ no fluxo de negócio nem a configuração necessária para mensageria distribuída.
  - A documentação afirma bounded contexts independentes e TDD, mas o código ainda mantém uma `WebApp.API` agregadora e o único teste identificado está falhando.

## Conclusão
O repositório já mostra uma intenção clara de evoluir para um ecossistema distribuído: há solução separada por contextos, projetos independentes para Auth/Alunos/Conteúdo/Pagamentos, BFF dedicado, `DbContext` por serviço, migrations por contexto, Swagger e uma base de mensageria com MassTransit/RabbitMQ. A Auth API também é o componente mais aderente ao escopo, com Identity, emissão de JWT e seed inicial de roles/usuário.

Os principais riscos técnicos ainda estão na **coerência arquitetural entre o desenho e a execução real**. O projeto mantém uma `WebApp.API` com comportamento monolítico e autenticação própria, o que conflita com a proposta de Auth centralizada e bounded contexts independentes. Além disso, o fluxo crítico de **matrícula → pagamento → ativação da matrícula** não está efetivamente distribuído por mensageria entre APIs; hoje ele depende majoritariamente de eventos em memória via MediatR. Também há inconsistências fortes na autenticação entre serviços: Auth emite JWT simétrico, Alunos/BFF esperam JWKS e Pagamentos/WebApp validam outro conjunto de `Issuer/Audience/Secret`.

Nos casos de uso do domínio, há implementação parcial: login, cadastro de curso e consulta de pagamento existem; porém o registro de progresso da aula não parte da Conteúdo API para Alunos API como definido no escopo, e a finalização do curso gera certificado sem validar progresso. Somado a isso, a Alunos API não registra todos os handlers necessários no startup, o BFF está restrito ao contexto de alunos e o único teste automatizado identificado falha.

Em resumo: a base do projeto é promissora, mas ainda precisa consolidar a separação real entre serviços, unificar a estratégia de autenticação, remover a duplicidade monolítica da `WebApp.API`, completar os fluxos distribuídos com mensageria/resiliência entre APIs e alinhar a documentação com a implementação executável.

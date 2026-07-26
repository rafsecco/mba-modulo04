# Feedback – Avaliação Geral

## Organização e Estrutura do Projeto

- **Pontos positivos:**
  - Separação clara e coerente de responsabilidades na raiz: `src/` (com `services/`, `api-gateways/`, `building-blocks/`), `tests/`, `docker/` e `k8s/`.
  - Arquivo de solução `TelesEducacao.sln` presente na raiz.
  - Dockerfiles individuais por serviço, centralizados em `docker/` (`Dockerfile.auth`, `Dockerfile.alunos`, `Dockerfile.conteudo`, `Dockerfile.pagamentos`, `Dockerfile.bff`).
  - Manifestos Kubernetes organizados por serviço em subpastas (`k8s/auth/`, `k8s/alunos/`, `k8s/conteudo/`, `k8s/pagamentos/`, `k8s/bff/`) com a infraestrutura isolada em `k8s/infra/`. Essa organização facilita a leitura e o `kubectl apply -R`.
  - Workflows presentes e separados por responsabilidade: `.github/workflows/ci.yml` e `.github/workflows/cd.yml`.
  - `.gitignore` correto: `[Bb]in/` e `[Oo]bj/` (linhas 24-25 e 88-89), `[Tt]est[Rr]esult*/` (linha 102) e `coverage*.xml` (linha 207) estão excluídos. **Verificado via `git ls-files`: nenhum binário, `bin/`, `obj/` ou artefato de build está versionado.**
  - Presença de `.editorconfig` e `.gitattributes` (commits `c6ff68b` e `4ab7569`), garantindo consistência de indentação e fim de linha — reforçado pelo lint no CI.
  - Os 5 contextos exigidos pelo escopo estão presentes: Auth, Conteúdo, Alunos, Pagamentos e BFF.

- **Pontos negativos:**
  - A pasta `tests/` contém **um único projeto de teste** (`TelesEducacao.Conteudo.Domain.Tests`) para aproximadamente 20 projetos na solução. A estrutura de testes não acompanha a estrutura da solução.
  - Inconsistência entre nome de pasta e nome de assembly: a pasta `src/services/TelesEducacao.Conteudo.Domain/` contém o projeto `TelesEducacao.Conteudos.Domain.csproj` (plural). O mesmo ocorre em `Conteudo.Data` / `Conteudos.Data` e `Conteudo.Application` / `Conteudos.Application`. Isso dificulta a navegação e a busca no repositório.

---

## Pipeline CI/CD

- **Pontos positivos:**
  - `ci.yml` bem estruturado e enxuto: checkout → setup .NET 9 → restore → build (`-c Release`) → lint → test (`ci.yml:17-35`).
  - **Lint/análise estática implementado** via `dotnet format --verify-no-changes --severity error` (`ci.yml:32`), que quebra o build em caso de violação. É um diferencial que muitos projetos não entregam.
  - `cd.yml` usa **matrix strategy** para os 5 serviços (`cd.yml:13-30`), evitando duplicação de jobs — solução elegante e de fácil manutenção.
  - Deploy no Docker Hub funcional com `docker/login-action@v3` e `docker/build-push-action@v6` (`cd.yml:39-55`).
  - **Boa prática de tagging:** cada imagem é publicada com `:latest` **e** `:${{ github.sha }}` (`cd.yml:52-53`), permitindo rastreabilidade e rollback.
  - Uso de cache do GitHub Actions com escopo por serviço (`cache-from`/`cache-to` com `scope=${{ matrix.service }}`, `cd.yml:54-55`), acelerando builds.
  - Credenciais corretamente referenciadas via `${{ secrets.DOCKERHUB_USERNAME }}` / `${{ secrets.DOCKERHUB_TOKEN }}` — **nenhuma credencial de registry exposta no workflow**.
  - Actions fixadas em major versions atualizadas (`checkout@v4`, `setup-dotnet@v4`, `buildx@v3`).
  - **Uso adequado de Pull Requests**, confirmado via `gh`: 6 PRs abertos (#1 a #6), 4 merged, com branches temáticas (`feat/1-health-checks`, `feat/2-CI-CD`, `feat/3-kubernetes`, `feat/fix-dependecies-services`). O CI executa em `pull_request` e em `push` de branches de feature, e o CD executa em `push` na `main` — **todas as execuções listadas em `gh run list` estão com status `success`**. Há ainda revisão automatizada via Copilot Code Review nos PRs.

- **Pontos negativos:**
  - **O `cd.yml` NÃO executa build de validação nem testes antes de publicar as imagens no Docker Hub.** O job `build-and-push` (`cd.yml:10-32`) não possui `needs:` apontando para um job de testes, e o `ci.yml` está explicitamente configurado para **não** rodar na `main` (`branches-ignore: - main`, `ci.yml:5-6`). Consequência prática: um merge que quebre os testes publica imagens quebradas em produção sem qualquer barreira. Este é o defeito mais relevante do pipeline.
  - **O CI não coleta cobertura de código.** O passo de teste (`ci.yml:35`) é apenas `dotnet test --no-build -c Release`, sem `--collect:"XPlat Code Coverage"`, sem geração de relatório e, principalmente, **sem gate de cobertura mínima**. O pacote `coverlet.collector` já está referenciado no projeto de teste, então o custo de adicionar seria mínimo.
  - Ausência de análise estática de verdade. `dotnet format` valida formatação/estilo, não qualidade nem segurança. Não há analisadores (`TreatWarningsAsErrors`, Roslyn analyzers), CodeQL ou SonarQube.
  - Ausência de scan de vulnerabilidades nas imagens publicadas (ex.: Trivy, Grype) e nas dependências NuGet.
  - Não há validação dos manifestos Kubernetes no pipeline (ex.: `kubeconform`, `kubectl apply --dry-run`), que teria detectado o problema do placeholder `<DOCKERHUB_USERNAME>` descrito adiante.
  - O build gera warnings `CS8618` (nullable) que passam silenciosamente — ver seção Qualidade do Código.

---

## Containerização

- **Pontos positivos:**
  - **Todos os 5 Dockerfiles usam multi-stage build** corretamente: estágio `sdk:9.0 AS build` e estágio final `aspnet:9.0` apenas com o publish (`Dockerfile.auth:2` e `:23`). O runtime não carrega o SDK.
  - **Imagens base oficiais e atualizadas** (`mcr.microsoft.com/dotnet/sdk:9.0` e `mcr.microsoft.com/dotnet/aspnet:9.0`), coerentes com o `TargetFramework net9.0` dos projetos.
  - **Layers otimizadas com cache inteligente:** os `.csproj` são copiados primeiro e o `dotnet restore` é executado antes do `COPY src ./` (`Dockerfile.auth:6-17`). Isso evita restore desnecessário quando apenas o código muda — prática correta e bem comentada.
  - `restore` feito por projeto e não pela solution (`Dockerfile.auth:14`), reduzindo o escopo do build de cada imagem.
  - **Execução como usuário não-root** em todos os Dockerfiles (`adduser --disabled-password ... && chown -R appuser:appuser /app` + `USER appuser`, `Dockerfile.auth:27-29`). Esta é uma prática de segurança acima do esperado para o módulo e merece destaque.
  - `EXPOSE` e `ENTRYPOINT` definidos corretamente por serviço, com portas distintas e coerentes (5101, 5201, 5301, 5401, 5035).
  - `.dockerignore` presente na raiz, excluindo `**/bin/`, `**/obj/`, `**/.git/`, `tests/` e `**/.env*` — reduz o contexto de build e evita vazamento de arquivos.
  - `docker-compose.yml` com **healthchecks reais** para RabbitMQ (`rabbitmq-diagnostics ping`) e SQL Server (`sqlcmd -Q "SELECT 1"`) e `depends_on` com `condition: service_healthy` (`docker-compose.yml:12-17`, `29-34`, `45-49`) — a stack sobe em ordem correta.
  - `.env.example` fornecido e `**.env` ignorado no `.gitignore` (linha 1); o compose parametriza credenciais via `${MSSQL_SA_PASSWORD}` / `${RABBITMQ_DEFAULT_PASS}`.
  - Imagens efetivamente publicadas: o workflow CD executou com sucesso em pushes na `main` (verificado em `gh run list`).

- **Pontos negativos:**
  - **A parametrização via `.env` é ilusória para as APIs.** O `docker-compose.yml` passa `${MSSQL_SA_PASSWORD}` apenas para o container `database` (`:25`); os containers das APIs recebem somente `ASPNETCORE_ENVIRONMENT` e `ASPNETCORE_URLS` (`:42-44`), lendo a connection string do `appsettings.Docker.json` embutido. **Resultado: alterar a senha no `.env` quebra a stack**, pois o banco sobe com a nova senha e as APIs continuam tentando a antiga. O procedimento "Alterar senha do SQL Server" documentado em `docker/README.md` não funciona. O correto seria injetar `ConnectionStrings__DefaultConnection` como variável de ambiente no compose, como já é feito — corretamente — nos manifestos Kubernetes.
  - Imagens base usam tags flutuantes (`sdk:9.0`, `aspnet:9.0`) sem pin por digest. Aceitável no contexto acadêmico, mas compromete builds reprodutíveis.
  - Ausência de instrução `HEALTHCHECK` nos Dockerfiles (mitigado pelo compose e pelos probes do K8s).

---

## Orquestração Kubernetes

- **Pontos positivos:**
  - Conjunto de manifestos **completo e coerente**: `namespace.yaml`, `configmap-app-config.yaml`, `secret-app-secrets.yaml`, além de Deployment + Service para os 5 serviços e para a infraestrutura (SQL Server e RabbitMQ), com PVCs (`k8s/infra/sqlserver-pvc.yaml`, `rabbitmq-pvc.yaml`).
  - Todos os recursos isolados no namespace `teleseducacao`.
  - **Liveness e readiness probes presentes nos 7 Deployments** (verificado individualmente), apontando para `/health/live` e `/health/ready` nas portas corretas (ex.: `auth-deployment.yaml:63-74`).
  - **`resources` com `requests` e `limits` de CPU e memória em todos os Deployments** (ex.: `auth-deployment.yaml:75-81`) — item frequentemente esquecido e aqui bem executado.
  - **Excelente separação entre configuração e segredo:** dados não sensíveis vêm de `configMapKeyRef` (ambiente, URLs internas) e credenciais/connection strings de `secretKeyRef` (`auth-deployment.yaml:36-62`). É exatamente o padrão esperado.
  - **Consistência verificada entre ConfigMap/Secret e os Services:** o Secret aponta para `Server=database,1433` e o Service do SQL Server chama-se de fato `database` (`k8s/infra/sqlserver-service.yaml:4`); o ConfigMap aponta para `http://auth:5101`, `http://alunos:5201`, `http://conteudo:5301`, `http://pagamentos:5401` e todos os Services correspondentes existem com esses nomes e portas. Não há divergência de DNS interno.
  - `initContainers` com `busybox:1.36` aguardando `database:1433` e `rabbitmq:5672` antes de subir a aplicação (`auth-deployment.yaml:20-28`) — solução pragmática e eficaz para o problema de ordem de inicialização, já que as APIs rodam migrations no startup.
  - `k8s/README.md` documenta o passo a passo, tabela de portas/probes e troubleshooting.

- **Pontos negativos:**
  - **Os manifestos não são aplicáveis como estão.** Os 5 Deployments de aplicação referenciam `image: <DOCKERHUB_USERNAME>/teleseducacao-<servico>:latest` (`bff/bff-deployment.yaml:31`, `auth/auth-deployment.yaml:31`, e idem em alunos, conteudo, pagamentos). `<` e `>` são caracteres inválidos em uma referência de imagem: o `kubectl apply -f k8s/ -R` cria os Deployments, mas **os pods falham com `InvalidImageName` e nunca sobem**. O `k8s/README.md:36-48` reconhece o problema e manda rodar um `sed` manual, mas o próprio README também sugere "tudo de uma vez" com `kubectl apply -f k8s/ -R` (`k8s/README.md:64`), o que não funciona. O escopo pede que os serviços rodem no cluster; do jeito atual, o deploy exige edição manual dos arquivos. O caminho idiomático seria usar Kustomize (`images:` overlay) ou um placeholder substituído pelo pipeline.
  - **`replicas: 1` em todos os Deployments** e ausência de HPA. O escopo cita escalabilidade; ela não está demonstrada. Nenhum serviço stateless (as 4 APIs e o BFF) precisaria ficar em réplica única.
  - `imagePullPolicy: IfNotPresent` combinado com a tag `:latest` (`bff-deployment.yaml:31-32`): o cluster não baixará novas versões de `latest` já presentes no nó. Como o CD publica também `:${{ github.sha }}`, o ideal seria referenciar a tag imutável do SHA.
  - O `initContainer` do **BFF aguarda `database:1433` e `rabbitmq:5672`** (`bff/bff-deployment.yaml:27-28`), mas o BFF não possui banco nem barramento — ele só faz chamadas HTTP. É acoplamento desnecessário que atrasa a subida do gateway sem motivo.
  - SQL Server modelado como `Deployment` + PVC em vez de `StatefulSet` — aceitável para cluster local, mas conceitualmente inadequado para carga stateful.
  - Não há `Ingress`; o acesso depende de `port-forward` (`k8s/README.md:78`).
  - **A execução real em cluster não foi validada nesta revisão** (ver Observações finais). A análise dos manifestos foi estática.

---

## Resiliência e Observabilidade

- **Pontos positivos:**
  - **Polly implementado de fato**, não apenas declarado: política de retry com backoff escalonado de 1s, 5s e 10s sobre `HandleTransientHttpError()` (`src/building-blocks/TelesEducacao.WebAPI.Core/Extensions/PollyExtensions.cs:9-21`).
  - **Circuit breaker configurado** nos HttpClients do BFF: `CircuitBreakerAsync(5, TimeSpan.FromSeconds(30))` (`src/api-gateways/TelesEducacao.Bff.Plataforma/Program.cs:36, 42, 48`).
  - **Health checks com a semântica correta**, o que é raro de ver bem feito (`src/building-blocks/TelesEducacao.WebAPI.Core/Extensions/HealthCheckExtensions.cs`):
    - `/health/live` usa `Predicate = _ => false` (`:40-43`) — liveness não depende de dependências externas, evitando que uma queda do banco cause reinício em cascata dos pods. **Está exatamente certo.**
    - `/health/ready` filtra por tag `ready` (`:45-48`), agregando SQL Server (`AddSqlServer`, `:30`) e RabbitMQ (`:33`).
  - **`RabbitMqHealthCheck` customizado e funcional** (`src/building-blocks/TelesEducacao.WebAPI.Core/HealthChecks/RabbitMqHealthCheck.cs`): força `EnsureConnectedAsync` e valida `PersistentConnectionState.Connected`, tratando exceção e retornando `Unhealthy` com motivo. É uma verificação real, não um `return Healthy()` decorativo.
  - Os probes do Kubernetes estão corretamente ligados a esses endpoints, fechando o ciclo entre aplicação e orquestrador.
  - Restart policy padrão do Kubernetes cobre o requisito de reinício automático; os `initContainers` reduzem o `CrashLoopBackOff` inicial.

- **Pontos negativos:**
  - **A política de retry é aplicada a apenas 1 dos 3 HttpClients do BFF.** Somente `IAlunoService` recebe `.AddPolicyHandler(PollyExtensions.EsperarTentar())` (`Bff.Plataforma/Program.cs:34`). `IAuthService` (`:38-42`) e `IConteudoService` (`:44-48`) têm circuit breaker mas **nenhum retry**. Considerando que a `PollyExtensions` já existe e está pronta, isso parece um esquecimento — e significa que falhas transitórias em Auth e Conteúdo não são reprocessadas.
  - **O readiness probe do BFF é vazio na prática.** O BFF chama `builder.Services.AddHealthChecks()` (`Program.cs:52`) em vez de `AddPlatformHealthChecks(...)`, portanto **nenhum check é registrado com a tag `ready`**. Como `MapPlatformHealthChecks` filtra `/health/ready` por essa tag (`HealthCheckExtensions.cs:47`), o endpoint retorna `Healthy` com o conjunto vazio, sempre. O readiness do BFF (`bff-deployment.yaml:74-79`) é decorativo: o pod é marcado pronto mesmo que Auth, Alunos e Conteúdo estejam todos fora do ar.
  - **Não há logs estruturados.** Nenhuma referência a Serilog, OpenTelemetry, Seq ou similar em qualquer `.csproj` (busca realizada em toda a `src/`). A aplicação usa apenas o logger padrão com `LogLevel` default nos `appsettings.json`. O escopo pede explicitamente "logs" como item de observabilidade.
  - **Não há métricas.** Nenhum endpoint Prometheus, `/metrics` ou instrumentação OpenTelemetry. O escopo cita "Observabilidade e resiliência, com restart policies, logs e métricas" — métricas estão **ausentes**.
  - A resiliência existe apenas na borda (BFF). A comunicação assíncrona via EasyNetQ/RabbitMQ não apresenta política explícita de retry, dead-letter queue ou tratamento de mensagens envenenadas.
  - O check de SQL Server é desabilitado em Development (`HealthCheckExtensions.cs:22`) — decisão razoável, apenas registrada.

---

## Qualidade do Código

- **Pontos positivos:**
  - Estrutura em camadas coerente por bounded context (`API`, `Application`, `Data`, `Domain`), com DDD e CQRS aplicados. **Não há superdimensionamento de arquitetura**: as camadas correspondem ao escopo herdado do módulo anterior, sem abstrações supérfluas.
  - Nomes descritivos e linguagem ubíqua em português consistente (`CriarCursoCommandHandler`, `CargaHorariaService`, `ConteudoProgramatico`).
  - **Seed de dados e migrações automáticas no startup implementados em todos os 4 serviços com banco** — item explicitamente verificado:
    - `TelesEducacao.Alunos.API/Program.cs:91` → `app.Services.UseDbMigrationAlunosHelper()`
    - `TelesEducacao.Conteudo.API/Program.cs:10` → `app.Services.UseDbMigrationConteudosHelper()`
    - `TelesEducacao.Pagamentos.API/Program.cs:86` → `app.Services.UseDbMigrationPagamentosHelper()`
    - `TelesEducacao.Auth.API/Program.cs:73` → `app.Services.UseDbMigrationAuthHelper()`
    Todos com `EnsureSeedData` + `MigrateAsync()` nos respectivos `Data/Configuration/DbMigrationHelpers.cs`.
  - `TelesEducacao.Conteudo.API/Program.cs` é o exemplo mais limpo da solução: 14 linhas delegando para extensions (`AddApiConfigurations`, `AddSwaggerConfigureServices`, `RegisterServices`) — bom padrão.
  - Reuso adequado via building blocks (`Core`, `WebAPI.Core`, `MessageBus`), evitando duplicação de health checks, JWT e Polly entre serviços.
  - `coverlet.collector` já configurado no projeto de teste (`TelesEducacao.Conteudo.Domain.Tests.csproj`).

- **Pontos negativos:**
  - **Cobertura de testes praticamente inexistente. Branch coverage = 5.8% (4 de 68 branches), contra o mínimo exigido de 80%.** Medição local com `dotnet test --collect:"XPlat Code Coverage"` + `reportgenerator`:
    ```
    Line coverage:   11%   (42 de 379)
    Branch coverage: 5.8%  (4 de 68)
    Method coverage: 13.1% (17 de 129)
    ```
    Apenas 2 assemblies são instrumentados (`TelesEducacao.Conteudos.Domain` 25.9% e `TelesEducacao.Core` 5.4%). **Os 5 serviços — Auth, Alunos, Conteúdo, Pagamentos e BFF — têm 0% de cobertura.** Nenhum handler, controller, repositório ou serviço de aplicação é testado.
  - **A solução inteira possui 1 (um) teste.** O arquivo `tests/TelesEducacao.Conteudo.Domain.Tests/CursoTests.cs` contém um único `[Fact]` (`Curso_Validar_ValidacoesDevemRetornarExceptions`) que agrupa 5 asserts de validação. Além do volume, o teste concentra múltiplos cenários em um só método — o ideal seria `[Theory]`/`[InlineData]` ou testes separados, para que a falha de um cenário não mascare os seguintes.
  - **O `README.md:20` declara "TDD (Test Driven Development): Desenvolvimento orientado a testes para garantir a qualidade do código".** A afirmação não se sustenta: 1 teste e 5.8% de branch coverage são incompatíveis com TDD. Documentação não é evidência de implementação — e, neste caso, promete o que o código não entrega.
  - **O build não está limpo.** Warnings `CS8618` (nullable) em `src/services/TelesEducacao.Conteudo.Domain/Curso.cs:16` (propriedades `Nome`, `Descricao`, `ConteudoProgramatico`) e `Aula.cs:16` (`Curso`). Não há `TreatWarningsAsErrors`, então passam despercebidos no CI.
  - Inconsistência de estilo entre os `Program.cs`: `Conteudo.API` usa extensions (14 linhas), enquanto `Bff.Plataforma/Program.cs` concentra 105 linhas inline (CORS, HttpClients, Polly, JWT, Swagger). Padronizar pelo modelo do Conteúdo melhoraria a legibilidade.
  - Divergência pasta × assembly (`Conteudo` × `Conteudos`) já citada, prejudicando a navegabilidade.
  - *Tolerância (mencionado, sem peso na avaliação):* código comentado em `src/services/TelesEducacao.Conteudo.API/Configurations/ApiConfig.cs` (linha do `RegisterServicesFromAssembly(typeof(CriarCursoCommandHandler).Assembly)` comentada).

---

## Segurança

- **Pontos positivos:**
  - **Autenticação JWT implementada com JWKS** (`AddJwtConfiguration`), com distribuição de chave assimétrica via endpoint `/jwks` (`AppSettings__AutenticacaoJwksUrl` no `configmap-app-config.yaml:8`). É uma abordagem superior ao compartilhamento de chave simétrica entre microsserviços.
  - **Containers rodando como usuário não-root** em todos os 5 Dockerfiles (`Dockerfile.auth:27-29`) — mitigação relevante de superfície de ataque.
  - Imagens base oficiais da Microsoft (`mcr.microsoft.com/dotnet/*`), na versão correspondente ao target framework.
  - Credenciais do Docker Hub corretamente geridas via GitHub Secrets (`cd.yml:42-43`), sem exposição no workflow.
  - Uso do recurso `Secret` do Kubernetes, com injeção via `secretKeyRef` e separação em relação ao ConfigMap (`auth-deployment.yaml:53-62`).
  - `.gitignore:1` ignora `**.env`; apenas `.env.example` é versionado — a intenção de proteger credenciais locais está correta.
  - Swagger com `AddSecurityDefinition`/`AddSecurityRequirement` Bearer JWT em todas as APIs.

- **Pontos negativos:**
  - **CORS totalmente permissivo**: política `"Total"` com `AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()` no BFF (`Bff.Plataforma/Program.cs:16-24`) e em `Conteudo.API/Configurations/ApiConfig.cs`, aplicada indistintamente em todos os ambientes.
  - **Validação de certificado TLS desabilitada**: `AllowSelfSignedCertificate()` aplicado aos HttpClients do BFF (`Program.cs:33, 40, 46`). Aceitável em desenvolvimento, mas não há condicional por ambiente — vale para produção também.
  - `TrustServerCertificate=True` em todas as connection strings, inclusive nas do Kubernetes.
  - Nenhum scan de vulnerabilidades de dependências (NuGet) ou de imagens no pipeline.

---

## Documentação

- **Pontos positivos:**
  - `README.md` raiz bem apresentado: badges, pilares técnicos, tabela de tecnologias, descrição dos bounded contexts, tabela de usuários de seed com credenciais (`README.md:93-97`) e portas de cada serviço — facilita muito a vida de quem clona o projeto.
  - **`docker/README.md` é uma documentação de qualidade**: pré-requisitos, passo a passo, o que sobe, como verificar saúde, acesso ao RabbitMQ Management, Swagger, comandos de limpeza e uma seção de troubleshooting realmente útil.
  - **`k8s/README.md` idem**: estrutura de pastas comentada, passo a passo, **tabela de portas e probes por serviço** (`k8s/README.md:85-93`) e troubleshooting para `CrashLoopBackOff`, `ImagePullBackOff` e lentidão do SQL Server.
  - Links cruzados do README raiz para as documentações de Docker e Kubernetes (`README.md:142-143`).
  - **Swagger ativo e configurado em todas as 5 APIs**, com definição de segurança JWT Bearer (verificado em cada `Program.cs`; no Conteúdo via `Configurations/SwaggerConfig.cs`).

- **Pontos negativos:**
  - **O `README.md` raiz ainda é o do módulo anterior.** A linha 8 afirma que o projeto é "focada no módulo de **Arquitetura, Modelagem e Qualidade de Software**" — que é o módulo 4. Este é o módulo de **DevOps**, e o README raiz **não menciona GitHub Actions, CI/CD, Docker Hub, pipeline ou a estratégia de branches em nenhum ponto**. A seção "6. Como Executar o Projeto" começa por `dotnet run` e trata Docker e Kubernetes como "Alternativas de Execução" (`README.md:140`), invertendo a prioridade do módulo. A entrega DevOps está documentada apenas nos sub-READMEs.
  - **`k8s/README.md:99` contradiz os manifestos**: afirma "Como os manifests **não usam** `initContainers` de espera, é normal que os primeiros pods reiniciem algumas vezes". Os manifestos **usam** `initContainers` (`auth-deployment.yaml:20-28` e nos 5 serviços). O texto ficou desatualizado após o PR #4/#6.
  - **`docker/README.md` afirma que sobe "SQL Server 2025"**, mas o `docker-compose.yml:20` usa `mcr.microsoft.com/mssql/server:2022-latest`.
  - **`docker/README.md` documenta comportamento que não existe**: diz que "As credenciais do RabbitMQ e SQL Server são lidas do arquivo `.env`" — falso para as APIs, que leem do `appsettings.Docker.json` embutido (o próprio README se contradiz linhas depois: "Toda a configuração de conexão está nos arquivos `appsettings.Docker.json`"). Por consequência, o procedimento "Alterar senha do SQL Server" documentado **quebra a stack** em vez de configurá-la.
  - **A Pagamentos API está ausente das listas de Swagger** tanto no `README.md:150-153` quanto na lista de Swagger do `docker/README.md`, embora o serviço exista e exponha Swagger na porta 5401.

---

## Conclusão

O projeto entrega uma **base DevOps sólida e bem executada na maior parte dos seus componentes de infraestrutura**. Os pontos fortes são consistentes e demonstram cuidado real, não apenas cumprimento formal de checklist:

- **Containerização exemplar**: os 5 Dockerfiles usam multi-stage build, ordenam as camadas para aproveitar cache, partem de imagens oficiais compatíveis com o target framework e — o que se destaca — **executam como usuário não-root**, prática de segurança acima do esperado para o módulo.
- **Manifestos Kubernetes maduros**: probes de liveness/readiness e `resources` com requests/limits em todos os 7 Deployments, separação correta entre ConfigMap e Secret, `initContainers` resolvendo ordem de inicialização, e consistência verificada entre nomes de Service e as URLs/connection strings configuradas.
- **Health checks com semântica correta**, incluindo o detalhe frequentemente errado de `/health/live` não depender de dependências externas — evitando reinícios em cascata. O `RabbitMqHealthCheck` customizado é uma verificação real.
- **Pipeline com boas escolhas**: matrix strategy, cache por serviço, tags `latest` + SHA, lint com `dotnet format` quebrando o build e segredos geridos via GitHub Secrets. O uso de Pull Requests com branches temáticas está adequado e todas as execuções estão verdes.
- **Migrações e seed automáticos** implementados e efetivamente invocados nos 4 serviços com banco.

Em contrapartida, há **três lacunas que precisam de atenção prioritária**, por comprometerem critérios centrais do escopo:

1. **Cobertura de testes: 5.8% de branch coverage contra os 80% exigidos.** A solução tem **um único teste**. Os 5 serviços têm 0% de cobertura. Agrava o quadro o fato de o `README.md:20` declarar a adoção de TDD — uma promessa que o código não sustenta. Este é, de longe, o ponto mais crítico do trabalho e o de maior distância em relação ao critério.
2. **O CD publica imagens no Docker Hub sem executar testes.** Como o `ci.yml` ignora a `main` (`branches-ignore`) e o `cd.yml` não declara `needs:` para um job de testes, **não existe barreira de qualidade antes da publicação**. Somado ao item 1, o pipeline está automatizado mas não está protegendo nada. Um `needs:` mais a coleta de cobertura com gate resolveriam ambos com pouco esforço — o `coverlet.collector` já está no projeto.
3. **Os manifestos Kubernetes não são aplicáveis sem edição manual**, por conta do placeholder `<DOCKERHUB_USERNAME>` nas 5 imagens, que faz os pods falharem com `InvalidImageName`. Kustomize ou substituição via pipeline resolveriam de forma idiomática.

Merecem correção também: a **ausência total de logs estruturados e métricas** (a observabilidade hoje se resume aos health checks, e "métricas" é item explícito do escopo); o **retry do Polly aplicado a apenas 1 dos 3 HttpClients** do BFF; o **readiness probe vazio do BFF** (`AddHealthChecks()` no lugar de `AddPlatformHealthChecks`); e a **documentação desatualizada** — README raiz ainda descrevendo o módulo 4 e sem qualquer menção ao pipeline, além de três afirmações que contradizem o código (`initContainers`, "SQL Server 2025" e a leitura de credenciais do `.env`).

Um ponto conceitual que vale registrar: **a parametrização de credenciais via `.env` no docker-compose é aparente**. Como as APIs leem a connection string do `appsettings.Docker.json` embutido na imagem, alterar o `.env` quebra a stack. Curiosamente, os manifestos Kubernetes **acertam** justamente o que o compose erra, injetando `ConnectionStrings__DefaultConnection` por `secretKeyRef` — vale trazer esse mesmo padrão para o compose.

**Em resumo:** a infraestrutura (Docker, Kubernetes, health checks, resiliência de borda) está em bom nível e revela domínio técnico dos conceitos do módulo. As fragilidades concentram-se em **garantia de qualidade automatizada** (testes e o gate ausente no CD), **observabilidade além dos health checks** (logs estruturados e métricas) e **higiene de documentação**. São lacunas objetivas e de correção bem delimitada — nenhuma exige rearquitetura, apenas complemento do que já está estruturado. O caminho para uma entrega completa está claro e bem pavimentado.

---

## Observações finais sobre a metodologia desta revisão

- **Build:** compila com sucesso (`dotnet build`), com warnings `CS8618` em `Curso.cs:16` e `Aula.cs:16`.
- **Testes:** 1 teste, 1 passou, 0 falharam.
- **Limitação do ambiente de avaliação:** a máquina de revisão possui apenas o runtime .NET 10 instalado, enquanto a solução tem como alvo o `net9.0`. A primeira execução de `dotnet test` abortou com `You must install or update .NET to run this application... Framework: 'Microsoft.NETCore.App', version '9.0.0'`. **Isso é uma limitação do ambiente do revisor e NÃO um defeito do projeto** — o CI (`ci.yml:23`) usa corretamente `dotnet-version: 9.0.x` e todas as execuções no GitHub Actions estão verdes. A medição foi refeita com `DOTNET_ROLL_FORWARD=Major`, obtendo sucesso.
- **Cobertura:** medida com `dotnet test --collect:"XPlat Code Coverage"` + `reportgenerator -reporttypes:TextSummary`, considerando o **Branch coverage** = **5.8%**.
- **Kubernetes:** **a execução real em cluster (Kind/Minikube) não foi validada** nesta revisão. A avaliação dos manifestos foi **estática**. A conclusão de que os pods falhariam com `InvalidImageName` decorre da análise da referência de imagem `<DOCKERHUB_USERNAME>/...`, que contém caracteres inválidos para um image reference.
- **Pipeline:** o histórico de PRs e execuções foi verificado via `gh pr list` e `gh run list` em `mba-dev-expert/mba-modulo05` (acesso bem-sucedido).
- **Artefatos de build:** o diretório `TestResults/` gerado durante a medição está corretamente coberto pelo `.gitignore:102` e foi removido ao final; nenhum arquivo do repositório foi alterado além deste `FEEDBACK.md`.

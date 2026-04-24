# Docker Compose - Teles Educação

## Configuração e Execução

### Pré-requisitos

- Docker Desktop instalado
- Docker Compose v2+

### Passo 1: Configurar variáveis de ambiente

Na pasta `docker/`, copie o arquivo de exemplo:

```bash
cp .env.example .env
```

O arquivo `.env` contém as variáveis padrão para desenvolvimento. Você pode editá-lo se desejar usar senhas diferentes:

```env
RABBITMQ_DEFAULT_USER=teleseducacao
RABBITMQ_DEFAULT_PASS=TelesEduca123!
ACCEPT_EULA=Y
MSSQL_SA_PASSWORD=YourStrong!Passw0rd
MSSQL_PID=Express
```

### Passo 2: Iniciar os serviços

Execute dentro da pasta `docker/`:

```bash
docker compose -f docker-compose.yml up -d --build
```

### O que será iniciado

- **RabbitMQ** (port 5672, Management 15672)
- **SQL Server 2025** (port 1433)
- **Auth API** (port 5101)
- **Alunos API** (port 5201)
- **Conteúdo API** (port 5301)
- **Pagamentos API** (port 5401)
- **BFF Plataforma** (port 5035)

### Verificando a saúde dos serviços

Aguarde alguns minutos para que todos os serviços iniciem. Os logs mostrarão o progresso. As APIs estarão prontas quando exibirem:

```
Now listening on: http://0.0.0.0:5XXX
Application started. Press Ctrl+C to shut down.
```

### Acessando os serviços

#### RabbitMQ Management
- URL: `http://localhost:15672`
- Usuário: `teleseducacao`
- Senha: `TelesEduca123!`

#### Swagger das APIs
- Auth: `http://localhost:5101/swagger`
- Alunos: `http://localhost:5201/swagger`
- Conteúdo: `http://localhost:5301/swagger`
- Pagamentos: `http://localhost:5401/swagger`
- BFF: `http://localhost:5035/swagger`

### Parar os serviços

```bash
docker compose down
```

### Remover volumes (limpeza completa)

```bash
docker compose down -v
```

## Configuração de Ambiente

### Arquivos appsettings.Docker.json

Cada serviço possui um arquivo `appsettings.Docker.json` com as configurações corretas para execução em Docker:

- `ASPNETCORE_ENVIRONMENT=Docker` ativa a leitura desses arquivos
- As conexões apontam para os nomes dos serviços do Docker Compose (ex: `database`, `rabbitmq`, `auth`)
- As credenciais do RabbitMQ e SQL Server são lidas do arquivo `.env`

### Variáveis de Ambiente

As variáveis de ambiente no `docker-compose.yml` são mínimas:
- `ASPNETCORE_ENVIRONMENT`: Define qual arquivo `appsettings.{Environment}.json` será carregado
- `ASPNETCORE_URLS`: Define a porta HTTP

Toda a configuração de conexão está nos arquivos `appsettings.Docker.json` de cada serviço.

## Troubleshooting

### As APIs não conseguem conectar ao banco de dados

Verifique se o serviço `database` está saudável. Execute:

```bash
docker compose logs database
```

Aguarde a mensagem `MSSQL is now ready to accept connections`.

### RabbitMQ está em loop de reinicialização

O RabbitMQ pode levar tempo para iniciar. Verifique com:

```bash
docker compose logs rabbitmq
```

### Erro "Connection refused" nas APIs

Certifique-se de que todos os serviços iniciaram completamente. Aguarde alguns minutos e verifique os logs:

```bash
docker compose logs auth
docker compose logs alunos
```

### Alterar senha do SQL Server

1. Edite o arquivo `.env` e atualize `MSSQL_SA_PASSWORD`
2. Execute `docker compose down -v` para remover o volume anterior
3. Execute `docker compose up --build` novamente

## Desenvolvimento

### Reconstruir um serviço específico

```bash
docker compose up --build auth
```

### Ver logs em tempo real

```bash
docker compose logs -f [nome-do-serviço]
```

Exemplo: `docker compose logs -f alunos`

### Executar comando em um container

```bash
docker compose exec [serviço] [comando]
```

Exemplo: `docker compose exec database /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "SELECT 1"`

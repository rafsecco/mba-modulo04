# **Teles Educação - Plataforma Educacional**

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=flat-square&logo=microsoft-sql-server&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=flat-square&logo=swagger&logoColor=black)

## **1. Apresentação**
Bem-vindo ao repositório do projeto **Teles Educação**. Esta plataforma é o resultado prático do MBA DevXpert Full Stack .NET, focada no módulo de **Arquitetura, Modelagem e Qualidade de Software**.

O projeto consiste no desenvolvimento de uma plataforma educacional robusta, aplicando padrões de design de software modernos para gerir eficientemente conteúdos, alunos e processos financeiros, utilizando uma arquitetura de microserviços.

---

## **2. Pilares Técnicos**
Para garantir uma aplicação escalável e de fácil manutenção, foram aplicados os seguintes conceitos:

* **DDD (Domain-Driven Design):** Modelagem orientada ao negócio com separação clara de domínios.
* **Bounded Contexts:** Cada contexto possui autonomia total e isolamento de responsabilidades.
* **CQRS:** Segregação de responsabilidades de leitura e escrita.
* **TDD (Test Driven Development):** Desenvolvimento orientado a testes para garantir a qualidade do código.
* **ACL (Anti-Corruption Layer):** Implementada no contexto de pagamentos para proteger o domínio interno de integrações externas.
* **Microserviços:** Arquitetura distribuída com comunicação via Message Bus.
* **API Gateway/BFF:** Backend for Frontend para otimizar a experiência do usuário.

---

## **3. Tecnologias Utilizadas**
| Categoria | Tecnologia |
| :--- | :--- |
| **Linguagem** | C# 13 / .NET 9 |
| **Framework Web** | ASP.NET Core Web API |
| **ORM** | Entity Framework Core |
| **Banco de Dados** | SQL Server |
| **Mensageria** | RabbitMQ (via Message Bus) |
| **Segurança** | ASP.NET Core Identity & JWT (JSON Web Token) |
| **Documentação** | Swagger (OpenAPI) |

---

## **4. Estrutura do Projeto (Bounded Contexts)**
A solução foi desenhada seguindo a premissa de **Contextos Delimitados** e **Microserviços**. Cada BC possui as camadas necessárias para implementar as soluções de cada problema específico de negócio, funcionando de forma independente:

* **Building Blocks:**
  * **TelesEducacao.Core:** Interfaces, entidades base e notificações compartilhadas (*Shared Kernel*).
  * **TelesEducacao.MessageBus:** Implementação do barramento de mensagens para comunicação entre serviços.
  * **TelesEducacao.WebAPI.Core:** Componentes compartilhados para APIs (controllers base, autenticação, etc.).

* **API Gateways:**
  * **TelesEducacao.Bff.Plataforma:** Backend for Frontend para a plataforma, agregando dados de múltiplos serviços.

* **Serviços:**
  * **TelesEducacao.Alunos:** Gestão completa do ciclo de vida do aluno.
    * `API`, `Application`, `Data`, `Domain`: Regras de negócio, comandos/queries e endpoints REST.
  * **TelesEducacao.Auth:** Autenticação e autorização.
    * `API`, `Application`, `Data`: Gestão de usuários, roles e tokens.
  * **TelesEducacao.Conteudo:** Gestão pedagógica (Cursos e Aulas).
    * `API`, `Application`, `Data`, `Domain`: Modelagem e operações de conteúdo educacional.
  * **TelesEducacao.Pagamentos:** Processamento financeiro.
    * `AntiCorruption`, `API`, `Business`, `Data`: Regras de negócio, persistência e integração com gateways.
---

## **5. Funcionalidades Implementadas**
* **Gestão de Cursos e Aulas:** CRUD completo para administração de conteúdo educacional.
* **Gestão de Alunos:** Matrículas, progresso e certificados.
* **Autenticação e Autorização:** Registro de usuários, login e controle de acesso via Roles.
* **Processamento de Pagamentos:** Integração com gateways externos via Anti-Corruption Layer.
* **API RESTful:** Endpoints padronizados para integração com Front-ends ou Apps.
* **Mensageria:** Comunicação assíncrona entre serviços via Message Bus.
* **Documentação Interativa:** Interface Swagger para exploração em tempo real.

---

## **6. Como Executar o Projeto**

### **Pré-requisitos**
* .NET SDK 9.0
* SQL Server
* IDE de sua preferência (Visual Studio 2022, Rider ou VS Code)

### **Passos para Execução**

1.  **Clone o Repositório:**
    ```bash
    git clone https://github.com/rafsecco/mba-modulo04.git
    cd mba-modulo04
    ```

2.  **Configuração do Banco de Dados:**
    * No arquivo `appsettings.json` (em cada serviço API), configure sua string de conexão com SQL Server.
    * O projeto possui configuração de **Seed**. Ao rodar, o banco será criado e populado automaticamente.

3.  **Usuários de Teste (Seed):**
    | Perfil | Email | Senha |
    | :--- | :--- | :--- |
    | Admin | `admin@mail.com` | `Dev@123` |
    | Aluno | `aluno1@mail.com` | `Dev@123` |
    | Aluno | `aluno2@mail.com` | `Dev@123` |

4.  **Executar os Serviços:**

    * Em Ambiente **Development** (padrão e SQLite):
      * Inicie o RabbitMQ com a configuração abaixo (docker):
        ```
        docker run -d \
          --name rabbitmq \
          -p 5672:5672 \
          -p 15672:15672 \
          -e RABBITMQ_DEFAULT_USER=teleseducacao \
          -e RABBITMQ_DEFAULT_PASS=TelesEduca123! \
          rabbitmq:management-alpine
        ```
      * Execute cada serviço API individualmente:
        ```bash
        dotnet run --project src/services/TelesEducacao.Alunos.API
        dotnet run --project src/services/TelesEducacao.Auth.API
        dotnet run --project src/services/TelesEducacao.Conteudo.API
        dotnet run --project src/services/TelesEducacao.Pagamentos.API
        dotnet run --project src/api-gateways/TelesEducacao.Bff.Plataforma
        ```
     * Em Ambiente **Staging** (SQL Server):
       * Inicie o RabbitMQ (Igual ao passo para Development):
       * Inicir o serviço do SQL Server local
       * Execute cada serviço API individualmente:
          ```bash
          dotnet run --project src/services/TelesEducacao.Alunos.API --launch-profile "Staging"
          dotnet run --project src/services/TelesEducacao.Auth.Api --launch-profile "Staging"
          dotnet run --project src/services/TelesEducacao.Conteudo.API --launch-profile "Staging"
          dotnet run --project src/services/TelesEducacao.Pagamentos.API --launch-profile "Staging"
          dotnet run --project src/api-gateways/TelesEducacao.Bff.Plataforma --launch-profile "Staging"
          ```

1.  **Acessar os Endpoints:**
    * **BFF (Frontend Gateway):** `http://localhost:5035`
    * **Alunos API:** `http://localhost:5201`
    * **Auth API:** `http://localhost:5101`
    * **Conteudo API:** `http://localhost:5301`
    
---

## **7. Documentação da API**
A documentação completa dos endpoints, modelos de entrada e saída pode ser acessada via Swagger após o início da aplicação nos endereços locais indicados acima.

  * **BFF (Frontend Gateway):** `http://localhost:5035/swagger/index.html`
  * **Alunos API:** `http://localhost:5201/swagger/index.html`
  * **Auth API:** `http://localhost:5101/swagger/index.html`
  * **Conteudo API:** `http://localhost:5301/swagger/index.html`
  

---

## **8. Avaliação e Contribuições**
* Este é um projeto acadêmico; contribuições externas não são aceitas no momento.
* Para dúvidas, utilize a aba de **Issues**.
* O arquivo `FEEDBACK.md` é reservado exclusivamente para as avaliações do instrutor.

---


### Desenvolvido por
- [Guilherme Sant'Anna](https://github.com/svcguilherme)
- [Jefferson Molaz](https://github.com/jmolaz)
- [Karollainny Teles](https://github.com/karollainnyteles)
- [Rafael Secco](https://github.com/rafsecco)

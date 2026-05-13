# SkillSnap

## Resumo do projeto

O SkillSnap é uma aplicação full-stack de portfólio construída com ASP.NET Core e Blazor WebAssembly. O sistema permite navegar por perfis de portfólio, visualizar projetos e habilidades, e gerenciar os dados por meio de uma API REST com Entity Framework Core e SQLite.

A solução está dividida em duas partes:
- `SkillSnap.Api`: API Web ASP.NET Core com Identity, autenticação JWT, cache e persistência com EF Core.
- `SkillSnap.Client`: front-end em Blazor WebAssembly com autenticação, estado de sessão e interface responsiva.

## Principais recursos

### CRUD e gerenciamento de dados
- Visualização e edição de perfis de portfólio.
- Listagem e criação de projetos.
- Listagem e criação de habilidades.
- Fluxo de seed e reset do banco para testes rápidos.
- Relacionamentos do EF Core com carregamento eficiente de dados relacionados usando `Include()` e `AsNoTracking()`.

### Segurança
- ASP.NET Identity para registro e login de usuários.
- Geração e validação de token JWT.
- Autorização baseada em roles para ações elevadas, como criação de projetos e skills.
- Armazenamento do token no local storage do navegador com reaproveitamento automático após recarregar a página.

### Cache e desempenho
- Cache em memória para consultas frequentes da API.
- Expiração de cache e lógica de fallback para reduzir carga no banco de dados.
- Atualização do cache após operações CRUD para manter os dados consistentes.
- Logs de hit, miss e refresh do cache para verificação.

### Gerenciamento de estado no Blazor
- Container de estado escopado para informações do usuário logado.
- Estado persistido para o perfil selecionado e contexto de edição.
- Compartilhamento de estado entre componentes sem recarregar a página.

## Processo de desenvolvimento e uso do Copilot

Este projeto foi construído de forma incremental com apoio do Copilot no planejamento, geração de código e revisão. O Copilot foi usado para:
- criar a base de controllers da API e páginas Blazor,
- implementar autenticação e tratamento de JWT,
- adicionar armazenamento do token no cliente e estado de sessão,
- refatorar componentes para melhorar o fluxo de dados,
- otimizar consultas da API e o uso de cache,
- polir a interface para telas desktop e mobile.

O fluxo de trabalho foi iterativo: implementar uma funcionalidade, compilar os dois projetos, corrigir problemas de compilação ou execução e depois refinar a experiência do usuário. O Copilot foi especialmente útil para acelerar código repetitivo, como classes de serviço, componentes de formulário e tratamento de respostas da API.

## Problemas conhecidos e melhorias futuras

### Problemas conhecidos
- Algumas telas de CRUD ainda são básicas e poderiam ter validação e mensagens de feedback melhores.
- A atribuição da role de administrador ainda depende de configuração no backend ou seed manual.
- O fluxo de atualização de perfil é simples e pode ser ampliado para lidar com múltiplos usuários de forma mais explícita.

### Melhorias futuras
- Adicionar um fluxo de seed adequado para criar uma conta administradora inicial.
- Substituir o processo atual de edição de perfil por métodos de API dedicados e serviços mais limpos.
- Adicionar testes unitários e de integração para autenticação, cache e gerenciamento de estado.
- Melhorar estados de carregamento, estados vazios e tratamento de erros na interface Blazor.
- Adicionar paginação ou busca para projetos e habilidades quando o volume de dados crescer.
- Considerar o uso de um `HttpMessageHandler` delegado para injetar o bearer token em vez de configurar os headers em serviços individuais.

## Como executar a solução

### API
```bash
dotnet run --project SkillSnap.Api
```

### Cliente
```bash
dotnet run --project SkillSnap.Client
```

Certifique-se de que a API esteja em execução antes de abrir o cliente.

## Observações

O projeto inclui:
- autenticação JWT
- autorização baseada em roles
- cache em memória com fallback
- interface Blazor responsiva
- persistência local de sessão

# 🧠 Roadmap — Agente Developer em C# com Qdrant

## Stack proposta

| Camada | Tecnologia |
|--------|-----------|
| Linguagem | C# / .NET 8 |
| Banco vetorial | **Qdrant** (Docker ou binário nativo Windows) |
| SDK | `Qdrant.Client` (oficial, .NET) |
| Embeddings | **Gemini Embedding** (`gemini-embedding-001`, 768 dims, grátis via AI Studio) |
| LLM | OpenAI API / Azure OpenAI |
| Arquitetura | Console app + serviços modulares (DI) |

---

## Fase 0 — Preparar ambiente (1 dia)
- Instalar **.NET SDK 8** (atualmente só há runtime instalado)
- Instalar **Docker Desktop** ou rodar Qdrant nativo no Windows
- Rodar Qdrant: `docker run -p 6333:6333 qdrant/qdrant`
- Criar projeto: `dotnet new console -n AgentDeveloper`
- **Entregável:** `dotnet run` imprime "Hello" e Qdrant responde em `http://localhost:6333`

---

## Fase 1 — Fundação: conectar ao Qdrant (1-2 dias)
- Instalar pacotes: `Qdrant.Client`, `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.Configuration`
- Configurar coleção com dimensão **768** (Gemini Embedding)
- Implementar `VectorRepository` (upsert + delete + listar)
- **Conceitos de IA:** o que é um embedding, dimensão do vetor, métrica de distância (cosine)
- **Entregável:** app cria coleção, insere e lista pontos

---

## Fase 2 — Embeddings (1-2 dias)
- Implementar serviço de embeddings (interface `IEmbeddingService`)
- **Modelo:** Gemini Embedding (`gemini-embedding-001`, **768 dims**, `outputDimensionality: 768`, API gratuita via Google AI Studio)
- Necessário: API key gratuita do Google AI Studio (https://aistudio.google.com/apikey)
- **Nota:** `text-embedding-004` foi descontinuado; usar `gemini-embedding-001`
- **Entregável:** dado um texto → retorna um vetor `float[768]`

---

## Fase 3 — RAG / Busca de conhecimento (2-3 dias) ⭐
- Ingerir documentos/código em chunks → embedding → salvar no Qdrant
- Implementar `SearchAsync(query, k)` → retorna top-k similares
- Implementar pipeline RAG: buscar contexto → montar prompt → chamar LLM → responder
- **Conceitos:** chunking, similaridade coseno, threshold de relevância
- **Entregável:** perguntar sobre o código/documentos e receber resposta fundamentada

---

## Fase 4 — Memória do agente (2-3 dias)
- Persistir conversas/decisões como pontos vetoriais
- Armazenar metadados estruturados (`payload` no Qdrant: tipo, timestamp, tags)
- Implementar busca híbrida (filtro por payload + similaridade vetorial)
- **Conceitos:** memória de curto/longo prazo, filtros por metadados
- **Entregável:** agente lembra decisões passadas e usa como contexto

---

## Fase 5 — Seleção de ações (3-4 dias) ⭐
- Definir conjunto de ferramentas/ações do agente (ex: `ReadFile`, `EditFile`, `RunTest`, `Search`)
- Para cada ação, criar descrição + embedding canônico
- Dado um pedido, **embed + buscar qual ação é mais similar** → escolher a ferramenta
- **Conceitos:** função de recompensa, escolha de tool, `tool_calling`
- **Entregável:** agente recebe "edite o método X" e seleciona a ferramenta certa

---

## Fase 6 — Loop agêntico + avançado (opcional)
- Loop de execução: perceber → raciocinar → agir → observar (ReAct)
- Aprendizado por reforço: guardar resultado de cada ação (sucesso/falha) e ajustar
- **Conceitos:** ReAct, experiência replay, otimização de decisões com feedback
- **Entregável:** agente que melhora suas escolhas com o tempo

---

## Ordem de prioridade
1 → 3 (base + RAG) → 4 (memória) → 5 (seleção de ações) → 6 (avançado)

## Pré-requisitos de ambiente
1. Instalar **.NET SDK 8** (https://dotnet.microsoft.com/download)
2. Instalar **Docker Desktop** OU rodar Qdrant nativo (binário Windows)
3. Obter **API key gratuita do Gemini Embedding** (https://aistudio.google.com/apikey)

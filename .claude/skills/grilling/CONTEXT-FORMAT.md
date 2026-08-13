# CONTEXT.md Format

A glossary of the project's ubiquitous language and nothing else — not a spec, not a scratch pad, not a home for implementation decisions.

```md
# {Context name}

{One or two sentences: what this context is responsible for, and what it deliberately is not.}

## Glossary

### {Term}

{What it means here, in one to three sentences, including the distinction that makes it worth defining — usually what it is *not*, or which nearby term it gets confused with.}
```

## Writing entries

- Define the term as **this project** uses it, not as the industry does.
- **Name the confusion.** Most terms earn an entry because two words were used for one concept, or one word for two: "An **Order** is the customer's request. Once fulfilment starts it becomes a **Shipment** — never the same record."
- Keep terms **alphabetical**.
- A term meaning different things in two contexts is a sign the contexts are correctly separated. Define it in each, don't reconcile.
- Prefer the user's word. Invent a canonical term only when every candidate is overloaded, and note the rejected one: "(previously 'account' — ambiguous with User)".

Update entries the moment a term resolves, not in a batch. When meaning changes, edit in place — git carries the history.
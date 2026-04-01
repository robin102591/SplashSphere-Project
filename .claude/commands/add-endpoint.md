---
description: Add a new API endpoint with full CQRS stack
---

Add the following endpoint: $ARGUMENTS

Use the `backend` agent to:
1. Create the command/query record in Application/Features/{Feature}/
2. Create the FluentValidation validator (for commands)
3. Create the handler with proper tenant scoping
4. Create the DTO if needed
5. Add the endpoint in API/Endpoints/{Feature}Endpoints.cs
6. Update .claude/skills/api-inventory.md

If the endpoint needs a new entity or migration, flag it for the `database` agent.

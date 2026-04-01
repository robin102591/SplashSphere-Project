---
description: Implement a new feature end-to-end using specialized agents
---

Implement the following feature: $ARGUMENTS

Plan the implementation by:
1. Read the relevant spec file if one exists (PHASE15_FEATURES.md, CASHIER_SHIFT_FEATURE.md, etc.)
2. Use the `database` agent for entity configurations and migrations
3. Use the `backend` agent for domain entities, CQRS handlers, and endpoints
4. Use the `frontend` agent for pages, components, and hooks
5. Use the `qa` agent for critical test cases
6. Use the `docs` agent to update the changelog and inventories

Work through each agent in order. Pass context between them as needed.

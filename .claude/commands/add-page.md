---
description: Add a new frontend page with hooks and types
---

Add the following page: $ARGUMENTS

Use the `frontend` agent to:
1. Create the page component in the appropriate app (admin or POS)
2. Create or update the TanStack Query hooks in hooks/
3. Add any needed TypeScript types to packages/types/
4. Wire up navigation (sidebar for admin, pill nav for POS)
5. Update .claude/skills/page-inventory.md

If the page needs a new API endpoint, flag it for the `backend` agent.

# Agent Instructions

## AI Authorship Markers

Mark all AI-written code with comments naming the model that wrote it (e.g., `Claude Fable 5`), so developers know exactly what was written or edited by AI. Placement rules:

1. If the AI implemented only the body of a function (the signature was given by the user), place `//written by [CurrentAIModel]` inside the function as its first line — not above the signature.
2. If the AI wrote both the signature and the body of a function, place `//written by [CurrentAIModel]` directly above the signature.
3. If the AI only added or edited some lines of an existing body or CSS, append `//edited by [CurrentAIModel]` (or `//added by [CurrentAIModel]`) at the end of each changed line. If many lines of an existing function were changed, place a single `//edited by [CurrentAIModel]` at the beginning of the function instead of marking every line.
4. When editing existing code (adding/editing lines or editing a function body), also add a short comment explaining why the existing code was changed — unless the existing code was itself written by AI.
5. If the AI wrote the whole file, explicitly state this with a comment at the top of the file: `//this file is written by [CurrentAIModel]` (instead of marking individual functions).
6. If the change was made for a specific ticket (GitHub issue), append the link to the issue in parentheses to the marker comment, e.g. `//edited by Claude Fable 5 (https://github.com/hand2note/BadBeatTV.Frontend/issues/279)`. This applies to all marker forms (`written by`, `edited by`, `added by`, `this file is written by`).
7. In Razor markup, NEVER place a `@* ... *@` comment inside a tag — i.e., between the tag name and its closing `>` or `/>`, including next to an attribute on a multi-line tag. Razor treats such a comment as an attribute and throws `InvalidOperationException` at runtime. When marking an attribute line, place the comment on its own line directly above the whole tag (or after the closing `>`/`/>`), and name the attribute it refers to in the comment text.


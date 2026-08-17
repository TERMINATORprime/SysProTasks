# AI Prompts Log

A record of the prompts I gave to an AI assistant (Claude) while working on this take-home,
kept for transparent disclosure. This pairs with the "AI assistance" section of
[SOLUTION.md](SOLUTION.md).

**How the assistant was used:** sanity-checking my own logic, explaining/refreshing me on
tooling and patterns (EF, TVPs, stored procs, DI, env-var config), diagnosing errors I hit,
generating test **fixture data**, and drafting documentation (README, the SOLUTION.md
scaffold). All production C# and SQL, and all design decisions, are my own.

> **Fidelity note:** Part 1 was reconstructed from a summarised transcript, so the wording is
> approximate. Part 2 is verbatim. I should reconcile Part 1 against my own notes if exact
> wording matters.

---

## Part 1 — earlier session (wording approximate)

1. "please also do research on it"
2. "please walk me through the env-var driven part, it's been a while since I've done this"
3. "I've got `builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(conn));` instead of `builder.Services.AddPersistence(conn);`"
4. "command to create migration"
5. [pasted error] "Could not execute because the specified command or file was not found… `dotnet-ef` does not exist" — how to fix
6. "then to update" (apply the migration)
7. [pasted `SqlException` — Login failed for user 'sa'] "yet I can connect; `var conn = builder.Configuration.GetConnectionString("Default");` does get the correct details"
8. "No, by connect I mean I've added the exact conn string to DataGrip and it connects fine, I can see the db"
9. "I have another project that uses DACPAC and used EF to connect just fine"
10. "They are missing completely" (the password's special characters, after checking the env var)
11. "I want to move CsvHelper from CLI to SysPro.Application — what commands can I run?"
12. [pasted error] "`dotnet remove … package` — Could not find any project in …"
13. "You can check how far I've gotten — it's been more than 4 hours, the spec said ±4 hours. I feel like I'm only ~60% done."
14. "Walk me through / show me the 'layering is inverted' point"
15. "I moved it from API to Application so the CLI can use it — will change the namespace"
16. "The spec says ±4 hours but take as long as you need — is there a chance they grade on time taken?"
17. [screenshot of the ImportAudit table] "hmm, not quite what you gave me earlier"
18. "`if (!usedLineNo.Contains(lineNo) || line.Quantity <= 0 || …)` … this seems to be causing Applied to end up 0?"
19. "How do I dependency-inject `IOrdersRepository` into `AddScoped<IOrdersService, OrderServices>()`?"
20. [pasted error] "Unable to resolve service for type 'SysPro.API.Services.OrderServices' while activating OrdersController"
21. [pasted error] "The request matched multiple endpoints" (4 GET actions)
22. "No, that's correct — it gets one order and all of its lines"
23. "Can you go over what there is? I know I still have to do the unit tests"
24. "What do I put for the SQL `up`, and how do I unify the connection strings?"
25. "Where do I run `export ConnectionStrings__Default='…'`?"
26. "On the hardcoded path — I'll fix that / make it a CLI param. The DELETE+INSERT was for a staging-table approach. No NEW/CHANGED/UNCHANGED audit will have to do. Removed the compose file, no time. Fixed the `VersionNumber++`. Leaving `UnitPriceCents` as `decimal(18,6)`."
27. "How do I stop all connections to that db so I can drop it and start again?"
28. [pasted error] "InvalidOperationException: Sequence contains no matching element … RunScript … line 154"
29. "Can you re-read `InsertOrUpdateOrders` — it should be updating order lines if they have an OrderLineId"
30. "The sproc was supposed to use OrderLineId as the indicator to update — check now"
31. "I've added it, it was a quick add"
32. "How do I add program args for the Rider IDE for the CLI tool?"

---

## Part 2 — this session (verbatim)

1. "`public async Task<List<IngestOrderModel>> ProcessCsvContext(...)` can you check the logic here"
2. "Can you please edit the readme to include how to run, so things like setting dotnet secrets, exports etc"
3. "I fixed that appsetting bit"
4. "So tomorrow, will finish up the xUnit tests, integration test and then write solution.md then make a video"
5. "I've added the unit tests, just have the integration test left. With this it needs to commit data, which means by default couldn't run more than once as the test params would have changed"
6. "Yes please will do option 1"
7. "question is my sql sproc, the guid is a auto field, manually setting the id wouldn't do anything"
8. "can you regive me the table with the expected result again"
9. "value on leaving a few of these bugs in to show the unit tests working?"
10. "trying to run `dotnet run --project SysPro.CLI -- "…/csvData"` — Unhandled exception: `ConnectionStrings__Default is not set`. pc has restarted"
11. "the sql is on my server, its there"
12. "can you add to the readme for the unit tests. I ran `dotnet test …` and `dotnet test … --filter "Category=Integration"`. I fixed that alias issue as well"
13. "Prerequisites section is now slightly understated … can you update this bit?"
14. "now help me start SOLUTION.md"
15. "While I do that, can you please create a AIPrompts.md and put all the prompts I used in that file"

# Hvad er systemet målrettet mod
Hjemmesiden er et open‑source projekt, der har til formål at motivere frivillige til at engagere sig i foreningsarbejde ved hjælp af gamification. Systemet fungerer på baggrund af et pointsystem, hvor optjente point kan omsættes til “gaver” – eksempelvis kontante belønninger eller andre incitamenter, som foreningen vælger at tilbyde.
# Struktur for foreningerne
- En forening kan have flere afdelinger.
    - Hver afdeling kan have flere medlemmer.
        - Medlemmer kan tildeles opgaver.

# Migrations
To create a new migration, run the following command in the terminal from the `Organization.ApiService` project directory:
- ```dotnet ef migrations add Initial --context AppDbContext --project ../Organization.Infrastructure/Organization.Infrastructure.csproj --verbose```   
To apply migrations to the database, run:
- ```dotnet ef database update --context AppDbContext --project ../Organization.Infrastructure/Organization.Infrastructure.csproj --verbose```

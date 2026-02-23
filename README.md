# Hvad er systemet målrettet mod
Hjemmesiden er et open‑source projekt, der har til formål at motivere frivillige til at engagere sig i foreningsarbejde ved hjælp af gamification. Systemet fungerer på baggrund af et pointsystem, hvor optjente point kan omsættes til “gaver” – eksempelvis kontante belønninger eller andre incitamenter, som foreningen vælger at tilbyde.

# Beskrivelse af systemet
## Struktur for foreningerne
- En forening kan have flere afdelinger.
    - Hver afdeling kan have flere medlemmer.
        - Medlemmer kan tildeles opgaver.

## Opgaver (Task)
- Brugere med rettigheder kan oprette task, med point, som medlemmer kan påtage sig. Point tildeles et medlem når en administrator har godkendt at opgaven er løst.

## Gaver (Prize)
- Brugere med rettigheder kan oprette price, med en værdi, som medlemmer kan bytte sig til for optjente point.

## Brugere
- Brugere opdeles i grupper, som har rettigheder. Rettigheder bruges til visning og styringer af opgaver og gaver.

# Developers
## Migrations
To create a new migration, run the following command in the terminal from the `Organization.ApiService` project directory:
- ```dotnet ef migrations add Initial --context AppDbContext --project ../Organization.Infrastructure/Organization.Infrastructure.csproj --verbose```   
To apply migrations to the database, run:
- ```dotnet ef database update --context AppDbContext --project ../Organization.Infrastructure/Organization.Infrastructure.csproj --verbose```

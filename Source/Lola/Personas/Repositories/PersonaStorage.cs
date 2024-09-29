namespace Lola.Personas.Repositories;

public class PersonaStorage(IConfiguration configuration)
    : JsonFilePerTypeStorage<PersonaEntity>("personas", configuration),
      IPersonaStorage;

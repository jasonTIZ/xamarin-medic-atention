namespace MedicalAtention.API.DTOs;

public record LoginResponse(string Token, UserDto User);
public record UserDto(int Id, string Name, string Role);

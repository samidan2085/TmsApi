namespace TmsApi.Application.DTOs;
using System.ComponentModel.DataAnnotations;
public class UpdateCertificateRequest
{
    [Required]
    [RegularExpression(@"^CERT-\d{4}$",
        ErrorMessage = "Certificate Number must be in the format CERT-0001.")]
public required string SerialNumber {get;set;} 
public required string Title {get;set;}
public int StudentId {get;set;}
public int CourseId {get;set;}
}
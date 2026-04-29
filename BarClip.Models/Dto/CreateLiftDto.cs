
namespace BarClipApi.Models.Dto;
public class CreateLiftDto
{
    public double WeightKg { get; set; }
    public int Reps { get; set; }
    public bool Successful { get; set; }
    public string ExerciseName { get; set; }
}

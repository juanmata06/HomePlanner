using System.ComponentModel.DataAnnotations;

public class Task
{   
    [Key]
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime DueDate { get; set; }

    public bool IsDone { get; set; } = false;
}

// TODO:
// - reemplaza el IsDone por un atributo enum que se llame status, puede tener 3 valores, todo, doing y done

// - reemplaza DueDate por LastCompletedDate

// - crea:
//     - createdAt
//     - startDate
//     - endDate

// - agrega las foreign keys:
//    - created_by y es de tipo user
//    - assigned_to y es de tipo user
using System.ComponentModel.DataAnnotations;

namespace MockSchoolManagement.ViewModels
{
    public class EditRoleViewModel
    {
        public EditRoleViewModel() 
        {
            Users = new List<string>();
        }

        [Display(Name = "角色Id")]
        public string Id { get; set; }
        [Display(Name ="角色名称")]
        [Required(ErrorMessage ="角色名称是必须的！")]
        public string RoleName { get; set; }

        public List<string> Users { get; set; }
    }
}

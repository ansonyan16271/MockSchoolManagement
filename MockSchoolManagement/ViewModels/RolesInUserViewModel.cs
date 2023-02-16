namespace MockSchoolManagement.ViewModels
{
    public class RolesInUserViewModel
    {
        /// <summary>
        /// 用户拥有的角色列表
        /// </summary>
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}

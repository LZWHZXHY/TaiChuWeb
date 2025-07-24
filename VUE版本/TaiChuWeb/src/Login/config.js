import Home from './home/Home.vue'
import UserList from './user-manage/UserList.vue'
import RoleList from './right-manage/RoleList.vue'
import RightList from './right-manage/RightList.vue'
import TagList from './tag-manage/TagList.vue'
import CompanyData from './interview-manage/CompanyData.vue'
import CompanyList from './interview-manage/CompanyList.vue'
import StudentList from './student-manage/StudentList.vue'
import GradeList from './student-manage/GradeList.vue'


const routes =[
    {path:"/index",component:Home},
    {path:"/user-manage/list", component:UserList},
    {path:"/right-manage/rolelist", component:RoleList},
    {path:"/right-manage/rightlist", component:RightList},
    {path:"/tag-manage/list", component:TagList},
    {path:"/interview-manage/companylist", component:CompanyList},
    {path:"/interview-manage/companydata", component:CompanyData},
    {path:"/student-manage/studentlist", component:StudentList},
    {path:"/student-manage/gradelist", component:GradeList}
    
]


export default routes
import { createRouter, createWebHistory } from "vue-router"
import HomeView from "@/views/HomeView.vue"
import TimesheetsView from "@/views/TimesheetsView.vue"
import ProjectView from "@/views/ProjectsView.vue"
import UsersView from "@/views/admin/UsersView.vue"

export const AppPaths = {
    Home: "/",
    Timesheets: "/timesheets",
    Projects: "/projects",
    Users: "/admin/users",
}

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        {
            path: AppPaths.Home,
            name: "home",
            component: HomeView,
        },
        {
            path: AppPaths.Timesheets,
            name: "timesheets",
            component: TimesheetsView,
        },
        {
            path: AppPaths.Projects,
            name: "projects",
            component: ProjectView,
        },
        {
            path: AppPaths.Users,
            name: "users",
            component: UsersView,
        },
    ],
})

export default router

import { createRouter, createWebHistory } from "vue-router"
import HomeView from '../views/HomeView.vue'
import UsersView from "@/views/admin/UsersView.vue"

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        {
            path: '/',
            name: 'home',
            component: HomeView,
        },
        {
            path: '/admin/users',
            name: 'users',
            component: UsersView,
        },
    ],
})

export default router

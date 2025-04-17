import { ref, computed } from 'vue'
import { defineStore } from 'pinia'

export type UserIdentity = { id: string, email: string }

export const useUsersStore = defineStore("users", () => {
    const users = ref<UserIdentity[]>([
        { id: "001", email: "jane.doe@example.com" },
        { id: "002", email: "john.doe@example.com" },
    ])

    return { users }
})


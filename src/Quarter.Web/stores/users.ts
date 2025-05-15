import { type Ref, ref} from "vue"
import { defineStore } from "pinia"
import type { UserIdentity } from "@/models/user.ts"

export interface UserState {
    /** Identities for all the loaded system users. */
    users: Ref<UserIdentity[]>
}
/**
 * Store to manage the system users.
 *
 * While part of the main application, this will only be usable for users with sufficient capabilities and reachable
 * from the admin views of the application.
 */
export const useUsersStore = defineStore("users", (): UserState => {
    const users = ref<UserIdentity[]>([
        { id: "001", email: "jane.doe@example.com" },
        { id: "002", email: "john.doe@example.com" },
    ])

    return { users }
})


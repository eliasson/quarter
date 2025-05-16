/** Lightweight type for identifying a user. */
export type UserIdentity = { id: string, email: string }

/** The identity of the user before login (or if the login fails and user is in demo mode). */
export const AnonymousUserIdentity: UserIdentity = { id: "", email: "jane.doe@example.com" }


export class User {
    constructor(public readonly id: string, public readonly email: string) {}
}

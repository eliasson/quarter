import { HttpClient, type IHttpClient } from "@/utils/http-client.ts"
import { User } from "@/models/user.ts"

export interface IApiClient {
    getCurrentUser(): Promise<User>
}

export class ApiClient implements IApiClient {
    private readonly http: IHttpClient

    /**
     * Construct a new API client using a default HTTP client.
     * @param http Optional, the HTTP client to use (intended for testing, by default a fetch-based client will be used).
     */
    constructor(http?: IHttpClient) {
        this.http = http ?? new HttpClient()
    }

    async getCurrentUser(): Promise<User> {
        const resource = await this.http.get<UserResourceOutput>("/api/users/self")
        return inflateUser(resource)
    }
}

// While the following types are exported they should not be used outside the client implementation. They represent
// the resource output and input types (basically DTO:s) used in the HTTP protocol.
//
// These are just structs, and we want our client to work with instances of classes (OOP FTW!).
//
// Export is mainly to allow usage from test to easier detect data breaks.

export type IsoDate = string

export type UserResourceOutput = {
    id: string
    email: string
    created: IsoDate
    updated: null | IsoDate
}

export function inflateUser(resource: UserResourceOutput): User {
    return new User(resource.id, resource.email)
}

import type { IApiClient } from "@/utils/api-client.ts"
import type { User } from "@/models/user.ts"
import { ControllablePromise } from "@/utils/common.ts"

export class FakeApiClient implements IApiClient {
    public readonly getCurrentUserResponse: ControllablePromise<User>

    constructor() {
        this.getCurrentUserResponse = new ControllablePromise<User>()
    }

    getCurrentUser(): Promise<User> {
        return this.getCurrentUserResponse.promise
    }
}



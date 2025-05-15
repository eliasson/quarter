export interface IClientApi {
    getCurrentUser(): Promise<void>
}

export class ClientApi implements IClientApi {
    getCurrentUser(): Promise<void> {
        return Promise.resolve();
    }
}

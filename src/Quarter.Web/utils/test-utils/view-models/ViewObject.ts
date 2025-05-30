export type QWrapper = any

export class ViewObject {
    constructor(protected readonly wrapper: QWrapper) {
    }

    exists(): boolean {
        return this.wrapper.exists()
    }

    html(): string {
        return this.wrapper.html()
    }
}

import { ViewObject } from "@/utils/test-utils/view-models/ViewObject.ts"
import type { VueWrapper } from "@vue/test-utils"

export class ButtonViewObject extends ViewObject {
    constructor(
        w: VueWrapper,
        testId: string) {
        const target = w.find(`[data-testid=${testId}]`)
        super(target)
    }

    async click(): Promise<void> {
        return this.wrapper.trigger("click")
    }
}

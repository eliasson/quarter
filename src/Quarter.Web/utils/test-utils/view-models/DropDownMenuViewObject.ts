import { ViewObject } from "@/utils/test-utils/view-models/ViewObject.ts"
import type { VueWrapper } from "@vue/test-utils"
import { ButtonViewObject } from "@/utils/test-utils/view-models/ButtonViewObject.ts"

export class DropDownMenuViewObject extends ViewObject {
    constructor(
        w: VueWrapper,
        testId: string = "drop-down-menu") {
        const target = w.find(`[data-testid=${testId}]`)
        super(target)
    }

    trigger(): ButtonViewObject {
        return new ButtonViewObject(this.wrapper, "trigger")
    }

    /** Get the modal part of the drop-down menu. */
    modal(): ViewObject {
        const modal = this.wrapper.find("[data-testid=drop-down-menu-modal]")
        return new ViewObject(modal)
    }
}

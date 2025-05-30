import { describe, it, expect, beforeEach } from "vitest"
import { mount, VueWrapper } from "@vue/test-utils"
import DropDownMenu from "@/components/DropDownMenu.vue"
import { DropDownMenuViewObject } from "@/utils/test-utils/view-models/DropDownMenuViewObject.ts"

describe('useCurrentUser', () => {
    let wrapper: VueWrapper

    beforeEach(() => {
        wrapper = mountComponent()
    })

    describe("initially", () => {
        it("should not rendered the menu", () => {
            expect(ui().menu().modal().exists()).toBe(false)
        })

        it("should render the trigger slot", () => {
            const trigger = ui().menu().trigger()
            expect(trigger.exists()).toBe(true)
        })

        it("should use ", () => {
            ui().menu()
        })

        describe("when triggered", () => {
            beforeEach(() => ui().menu().trigger().click())

            it("should rendered the menu", () => {
                expect(ui().menu().modal().exists()).toBe(true)
            })
        })
    })

    function ui() {
        const menu = new DropDownMenuViewObject(wrapper)
        return {
            menu: () => menu,
        }
    }

    function mountComponent() {
        const props = {
            triggerIcon: "unit-test",
            triggerTitle: "TRIGGER",
            items: [],
        }

        return mount(DropDownMenu, { props })
    }
})

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
            const content = ui().menu().html()
            expect(content).toContain("TRIGGER")
        })

        describe("when triggered", () => {
            beforeEach(() => ui().triggerMenu())

            it("should rendered the menu", () => {
                expect(ui().menu().modal().exists()).toBe(true)
            })
        })
    })

    function ui() {
        const menu = new DropDownMenuViewObject(wrapper)
        return {
            menu: () => menu,
            triggerMenu: () => wrapper.find("button").trigger("click")
        }
    }

    function mountComponent() {
        const props = {
            items: []
        }

        const slots = {
            trigger: `<button type="button" @click="trigger">TRIGGER</button>`
        }

        return mount(DropDownMenu, { props, slots })
    }
})

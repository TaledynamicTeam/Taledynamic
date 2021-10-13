import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import { VueCookieNext } from 'vue-cookie-next'

import { 
    create, 
    NButton, NConfigProvider, NGlobalStyle, 
    NLayoutHeader, NMenu, NText, NSpace,
    NForm, NFormItem, NInput, NCheckbox,
    NCard, NTabs, NTabPane, NH1, NA, NGrid, NGridItem,
    NIcon, NLayout, NLayoutContent, NResult, NMessageProvider, 
    NAutoComplete, NTooltip
    
} from 'naive-ui'

const naive = create({
    components: [
        NButton, NConfigProvider, NGlobalStyle, 
        NLayoutHeader, NMenu, NText, NSpace, 
        NForm, NFormItem, NInput, NCheckbox,
        NCard, NTabs, NTabPane, NH1, NA, NGrid,
        NGridItem, NIcon, NLayout, NLayoutContent,
        NResult, NMessageProvider, NAutoComplete, 
        NTooltip
    ]
})

createApp(App).use(router).use(naive).use(VueCookieNext).mount('#app')
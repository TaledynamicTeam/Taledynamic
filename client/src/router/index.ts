import { InitializationStatus } from "@/models";
import { store } from "@/store";
import { createRouter, createWebHistory, RouteRecordRaw } from "vue-router";

const routes: Array<RouteRecordRaw> = [
  {
    path: "/",
    name: "Main",
    meta: { requiresAuth: true },
    component: () => import("@/views/MainView.vue")
  },
  {
    path: "/auth",
    name: "Auth",
    redirect: () => ({ name: "AuthSignIn" }),
    component: () => import("@/views/AuthView.vue"),
    children: [
      {
        path: "signin",
        name: "AuthSignIn",
        component: () => import("@/components/auth/SignInForm.vue")
      },
      {
        path: "signup",
        name: "AuthSignUp",
        component: () => import("@/components/auth/SignUpForm.vue")
      }
    ]
  },
  {
    path: "/account",
    name: "Account",
    component: () => import("@/views/ProfileView.vue"),
    meta: { requiresAuth: true },
    children: [
      {
        path: "settings",
        name: "AccountSettings",
        component: () => import("@/components/profile/SettingsSection.vue")
      },
      {
        path: "data",
        name: "AccountData",
        component: () => import("@/components/profile/DataSection.vue")
      }
    ]
  },
  {
    path: "/integration/telegram",
    name: "IntegrationTelegram",
    component: () => import("@/views/IntegrationTelegram.vue")
  },
  {
    path: "/table/:workspaceId/:tableId",
    name: "Table",
    props: true,
    meta: { requiresAuth: true },
    component: () => import("@/views/TableView.vue")
  },
  {
    path: "/:pathMatch(.*)*",
    name: "NotFound",
    component: () => import("@/views/NotFoundView.vue")
  }
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
});

router.beforeResolve(async (to) => {
  let isLoggedIn: boolean = store.getters["user/isLoggedIn"];
  if (!isLoggedIn) {
    isLoggedIn = await store.dispatch("user/init");
  }
  if (to.meta.requiresAuth && !isLoggedIn) {
    store.commit("pageError");
    return {
      name: "Auth",
      query: { redirect: to.fullPath }
    };
  } else if ((to.name === "AuthSignIn" || to.name === "AuthSignUp") && isLoggedIn) {
    return {
      name: "ProfileIndex"
    };
  }
  store.commit("pageReady");
});

router.afterEach(
  () =>
    new Promise<void>((resolve) => {
      const isLoggedIn: boolean = store.getters["user/isLoggedIn"];
      const workspacesInitStatus: InitializationStatus = store.getters["workspaces/initStatus"];
      if (isLoggedIn && workspacesInitStatus !== InitializationStatus.Success) {
        setTimeout(async () => {
          try {
            await store.dispatch("workspaces/init");
            resolve();
          } catch (error) {
            if (error instanceof Error) {
              console.log(error.message);
            }
          }
        }, 500);
      }
    })
);

export default router;

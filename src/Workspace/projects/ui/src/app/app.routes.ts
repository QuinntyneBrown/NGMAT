import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { Dashboard } from './pages/dashboard/dashboard';
import { Shell } from './layout/shell/shell';
import { authGuard, noAuthGuard } from './core';
import { Missions } from './pages/missions/missions';
import { MissionEditor } from './pages/mission-editor/mission-editor';
import { Users } from './pages/users/users';
import { UserDetail } from './pages/user-detail/user-detail';
import { Roles } from './pages/roles/roles';
import { RoleDetailComponent } from './pages/role-detail/role-detail';
import { Permissions } from './pages/permissions/permissions';
import { ApiKeys } from './pages/api-keys/api-keys';

export const routes: Routes = [
  {
    path: 'login',
    component: Login,
    canActivate: [noAuthGuard],
  },
  {
    path: '',
    component: Shell,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        component: Dashboard,
      },
      {
        path: 'missions',
        component: Missions,
      },
      {
        path: 'missions/new',
        component: MissionEditor,
      },
      {
        path: 'missions/:id',
        component: MissionEditor,
      },
      {
        path: 'missions/:id/edit',
        component: MissionEditor,
      },
      {
        path: 'users',
        component: Users,
      },
      {
        path: 'users/new',
        component: UserDetail,
      },
      {
        path: 'users/:id',
        component: UserDetail,
      },
      {
        path: 'roles',
        component: Roles,
      },
      {
        path: 'roles/new',
        component: RoleDetailComponent,
      },
      {
        path: 'roles/:id',
        component: RoleDetailComponent,
      },
      {
        path: 'permissions',
        component: Permissions,
      },
      {
        path: 'api-keys',
        component: ApiKeys,
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];

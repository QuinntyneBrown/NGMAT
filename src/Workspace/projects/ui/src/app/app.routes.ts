import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { Dashboard } from './pages/dashboard/dashboard';
import { Shell } from './layout/shell/shell';
<<<<<<< HEAD
import { authGuard, noAuthGuard } from './core';
=======
import { Missions } from './pages/missions/missions';
import { MissionEditor } from './pages/mission-editor/mission-editor';
>>>>>>> 4687e41e949cd2ff7844297f8db1bb620a145a0a

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


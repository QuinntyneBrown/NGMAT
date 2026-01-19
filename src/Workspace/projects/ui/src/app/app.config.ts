import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
<<<<<<< HEAD
import { provideHttpClient, withInterceptors } from '@angular/common/http';
=======
import { provideHttpClient, withFetch } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
>>>>>>> 4687e41e949cd2ff7844297f8db1bb620a145a0a

import { routes } from './app.routes';
import { authInterceptor } from './core';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
<<<<<<< HEAD
    provideHttpClient(withInterceptors([authInterceptor])),
  ],
=======
    provideHttpClient(withFetch()),
    provideAnimationsAsync()
  ]
>>>>>>> 4687e41e949cd2ff7844297f8db1bb620a145a0a
};

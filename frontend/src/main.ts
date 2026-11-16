import { registerLocaleData } from '@angular/common';
import localeHu from '@angular/common/locales/hu';
import { bootstrapApplication } from '@angular/platform-browser';
import { App } from './app/app';
import { appConfig } from './app/app.config';

registerLocaleData(localeHu);

bootstrapApplication(App, appConfig).catch((err) => console.error(err));

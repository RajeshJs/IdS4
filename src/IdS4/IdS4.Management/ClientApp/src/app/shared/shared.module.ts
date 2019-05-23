import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
// delon
import { AlainThemeModule } from '@delon/theme';
import { DelonABCModule } from '@delon/abc';
import { DelonACLModule } from '@delon/acl';
import { DelonFormModule } from '@delon/form';
// i18n
import { TranslateModule } from '@ngx-translate/core';

// #region third libs
import { NgZorroAntdModule } from 'ng-zorro-antd';
import { CountdownModule } from 'ngx-countdown';
const THIRDMODULES = [ NgZorroAntdModule, CountdownModule ];
// #endregion

// #region shared services
import { ConfigurationService } from './services/configuration.service';
import { StorageService } from './services/storage.service';
import { OidcService } from './services/oidc.service';
// #endregion

// #region your componets & directives
const COMPONENTS = [];
const DIRECTIVES = [];
// #endregion

@NgModule({
	imports: [
		CommonModule,
		FormsModule,
		RouterModule,
		ReactiveFormsModule,
		AlainThemeModule.forChild(),
		DelonABCModule,
		DelonACLModule,
		DelonFormModule,
		// third libs
		...THIRDMODULES
	],
	declarations: [
		// your components
		...COMPONENTS,
		...DIRECTIVES
	],
	exports: [
		CommonModule,
		FormsModule,
		ReactiveFormsModule,
		RouterModule,
		AlainThemeModule,
		DelonABCModule,
		DelonACLModule,
		DelonFormModule,
		// i18n
		TranslateModule,
		// third libs
		...THIRDMODULES,
		// your components
		...COMPONENTS,
		...DIRECTIVES
	]
})
export class SharedModule {
	static forRoot(): ModuleWithProviders {
		return {
			ngModule: SharedModule,
			providers: [ ConfigurationService, StorageService, OidcService ]
		};
	}
}

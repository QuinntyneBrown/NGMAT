import { Injectable } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig, MatSnackBarRef, TextOnlySnackBar } from '@angular/material/snack-bar';

export type SnackbarType = 'success' | 'warning' | 'error' | 'info';

@Injectable({
  providedIn: 'root',
})
export class SnackbarService {
  private readonly defaultDuration = 3000;

  private readonly panelClasses: Record<SnackbarType, string> = {
    success: 'g-snackbar-success',
    warning: 'g-snackbar-warning',
    error: 'g-snackbar-error',
    info: 'g-snackbar-info',
  };

  constructor(private snackBar: MatSnackBar) {}

  show(
    message: string,
    action?: string,
    duration?: number
  ): MatSnackBarRef<TextOnlySnackBar> {
    const config: MatSnackBarConfig = {
      duration: duration ?? this.defaultDuration,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
    };

    return this.snackBar.open(message, action, config);
  }

  showSuccess(
    message: string,
    action?: string,
    duration?: number
  ): MatSnackBarRef<TextOnlySnackBar> {
    return this.showWithType(message, 'success', action, duration);
  }

  showError(
    message: string,
    action?: string,
    duration?: number
  ): MatSnackBarRef<TextOnlySnackBar> {
    return this.showWithType(message, 'error', action, duration);
  }

  showWarning(
    message: string,
    action?: string,
    duration?: number
  ): MatSnackBarRef<TextOnlySnackBar> {
    return this.showWithType(message, 'warning', action, duration);
  }

  showInfo(
    message: string,
    action?: string,
    duration?: number
  ): MatSnackBarRef<TextOnlySnackBar> {
    return this.showWithType(message, 'info', action, duration);
  }

  dismiss(): void {
    this.snackBar.dismiss();
  }

  private showWithType(
    message: string,
    type: SnackbarType,
    action?: string,
    duration?: number
  ): MatSnackBarRef<TextOnlySnackBar> {
    const config: MatSnackBarConfig = {
      duration: duration ?? this.defaultDuration,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
      panelClass: [this.panelClasses[type]],
    };

    return this.snackBar.open(message, action, config);
  }
}

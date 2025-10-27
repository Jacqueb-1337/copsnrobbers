.class public final Lcom/kamcord/android/core/KC_n;
.super Ljava/lang/Object;

# interfaces
.implements Lcom/kamcord/android/core/KC_m;


# static fields
.field private static a:[I


# direct methods
.method static constructor <clinit>()V
    .locals 1

    const/4 v0, 0x1

    new-array v0, v0, [I

    sput-object v0, Lcom/kamcord/android/core/KC_n;->a:[I

    return-void
.end method

.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final a()Lcom/kamcord/android/core/KC_f;
    .locals 7

    const/4 v6, 0x0

    invoke-static {}, Ljavax/microedition/khronos/egl/EGLContext;->getEGL()Ljavax/microedition/khronos/egl/EGL;

    move-result-object v0

    check-cast v0, Ljavax/microedition/khronos/egl/EGL10;

    invoke-interface {v0}, Ljavax/microedition/khronos/egl/EGL10;->eglGetCurrentDisplay()Ljavax/microedition/khronos/egl/EGLDisplay;

    move-result-object v1

    const/16 v2, 0x3059

    invoke-interface {v0, v2}, Ljavax/microedition/khronos/egl/EGL10;->eglGetCurrentSurface(I)Ljavax/microedition/khronos/egl/EGLSurface;

    move-result-object v2

    const/16 v3, 0x3057

    sget-object v4, Lcom/kamcord/android/core/KC_n;->a:[I

    invoke-interface {v0, v1, v2, v3, v4}, Ljavax/microedition/khronos/egl/EGL10;->eglQuerySurface(Ljavax/microedition/khronos/egl/EGLDisplay;Ljavax/microedition/khronos/egl/EGLSurface;I[I)Z

    sget-object v3, Lcom/kamcord/android/core/KC_n;->a:[I

    aget v3, v3, v6

    const/16 v4, 0x3056

    sget-object v5, Lcom/kamcord/android/core/KC_n;->a:[I

    invoke-interface {v0, v1, v2, v4, v5}, Ljavax/microedition/khronos/egl/EGL10;->eglQuerySurface(Ljavax/microedition/khronos/egl/EGLDisplay;Ljavax/microedition/khronos/egl/EGLSurface;I[I)Z

    sget-object v0, Lcom/kamcord/android/core/KC_n;->a:[I

    aget v0, v0, v6

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v3, v0}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    return-object v1
.end method

.method public final b()Lcom/kamcord/android/core/KC_p;
    .locals 11

    const/16 v10, 0x3028

    const/16 v1, 0x1908

    const/4 v3, 0x0

    invoke-static {}, Ljavax/microedition/khronos/egl/EGLContext;->getEGL()Ljavax/microedition/khronos/egl/EGL;

    move-result-object v0

    check-cast v0, Ljavax/microedition/khronos/egl/EGL10;

    invoke-interface {v0}, Ljavax/microedition/khronos/egl/EGL10;->eglGetCurrentDisplay()Ljavax/microedition/khronos/egl/EGLDisplay;

    move-result-object v4

    const/16 v2, 0x3059

    invoke-interface {v0, v2}, Ljavax/microedition/khronos/egl/EGL10;->eglGetCurrentSurface(I)Ljavax/microedition/khronos/egl/EGLSurface;

    move-result-object v2

    sget-object v5, Lcom/kamcord/android/core/KC_n;->a:[I

    invoke-interface {v0, v4, v2, v10, v5}, Ljavax/microedition/khronos/egl/EGL10;->eglQuerySurface(Ljavax/microedition/khronos/egl/EGLDisplay;Ljavax/microedition/khronos/egl/EGLSurface;I[I)Z

    sget-object v2, Lcom/kamcord/android/core/KC_n;->a:[I

    aget v5, v2, v3

    const/4 v2, 0x0

    sget-object v6, Lcom/kamcord/android/core/KC_n;->a:[I

    invoke-interface {v0, v4, v2, v3, v6}, Ljavax/microedition/khronos/egl/EGL10;->eglGetConfigs(Ljavax/microedition/khronos/egl/EGLDisplay;[Ljavax/microedition/khronos/egl/EGLConfig;I[I)Z

    sget-object v2, Lcom/kamcord/android/core/KC_n;->a:[I

    aget v2, v2, v3

    new-array v6, v2, [Ljavax/microedition/khronos/egl/EGLConfig;

    sget-object v7, Lcom/kamcord/android/core/KC_n;->a:[I

    invoke-interface {v0, v4, v6, v2, v7}, Ljavax/microedition/khronos/egl/EGL10;->eglGetConfigs(Ljavax/microedition/khronos/egl/EGLDisplay;[Ljavax/microedition/khronos/egl/EGLConfig;I[I)Z

    sget-object v2, Lcom/kamcord/android/core/KC_n;->a:[I

    aget v7, v2, v3

    move v2, v3

    :goto_0
    if-ge v2, v7, :cond_2

    aget-object v8, v6, v2

    sget-object v9, Lcom/kamcord/android/core/KC_n;->a:[I

    invoke-interface {v0, v4, v8, v10, v9}, Ljavax/microedition/khronos/egl/EGL10;->eglGetConfigAttrib(Ljavax/microedition/khronos/egl/EGLDisplay;Ljavax/microedition/khronos/egl/EGLConfig;I[I)Z

    sget-object v9, Lcom/kamcord/android/core/KC_n;->a:[I

    aget v9, v9, v3

    if-ne v9, v5, :cond_1

    const/16 v2, 0x3025

    sget-object v5, Lcom/kamcord/android/core/KC_n;->a:[I

    invoke-interface {v0, v4, v8, v2, v5}, Ljavax/microedition/khronos/egl/EGL10;->eglGetConfigAttrib(Ljavax/microedition/khronos/egl/EGLDisplay;Ljavax/microedition/khronos/egl/EGLConfig;I[I)Z

    sget-object v2, Lcom/kamcord/android/core/KC_n;->a:[I

    aget v2, v2, v3

    const/16 v5, 0x3024

    sget-object v6, Lcom/kamcord/android/core/KC_n;->a:[I

    invoke-interface {v0, v4, v8, v5, v6}, Ljavax/microedition/khronos/egl/EGL10;->eglGetConfigAttrib(Ljavax/microedition/khronos/egl/EGLDisplay;Ljavax/microedition/khronos/egl/EGLConfig;I[I)Z

    sget-object v5, Lcom/kamcord/android/core/KC_n;->a:[I

    aget v5, v5, v3

    const/16 v6, 0x3026

    sget-object v7, Lcom/kamcord/android/core/KC_n;->a:[I

    invoke-interface {v0, v4, v8, v6, v7}, Ljavax/microedition/khronos/egl/EGL10;->eglGetConfigAttrib(Ljavax/microedition/khronos/egl/EGLDisplay;Ljavax/microedition/khronos/egl/EGLConfig;I[I)Z

    sget-object v0, Lcom/kamcord/android/core/KC_n;->a:[I

    aget v3, v0, v3

    const/4 v0, 0x5

    if-ne v5, v0, :cond_0

    const/16 v0, 0x1907

    :goto_1
    new-instance v1, Lcom/kamcord/android/core/KC_p;

    invoke-direct {v1, v2, v0, v3}, Lcom/kamcord/android/core/KC_p;-><init>(III)V

    move-object v0, v1

    :goto_2
    return-object v0

    :cond_0
    move v0, v1

    goto :goto_1

    :cond_1
    add-int/lit8 v2, v2, 0x1

    goto :goto_0

    :cond_2
    new-instance v0, Lcom/kamcord/android/core/KC_p;

    const/16 v2, 0x18

    invoke-direct {v0, v2, v1, v3}, Lcom/kamcord/android/core/KC_p;-><init>(III)V

    goto :goto_2
.end method

.method public final c()I
    .locals 1

    invoke-static {}, Ljavax/microedition/khronos/egl/EGLContext;->getEGL()Ljavax/microedition/khronos/egl/EGL;

    move-result-object v0

    check-cast v0, Ljavax/microedition/khronos/egl/EGL10;

    invoke-interface {v0}, Ljavax/microedition/khronos/egl/EGL10;->eglGetCurrentContext()Ljavax/microedition/khronos/egl/EGLContext;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/Object;->hashCode()I

    move-result v0

    return v0
.end method

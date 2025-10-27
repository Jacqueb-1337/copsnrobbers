.class public final Lcom/kamcord/android/core/KC_o;
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

    sput-object v0, Lcom/kamcord/android/core/KC_o;->a:[I

    return-void
.end method

.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final a()Lcom/kamcord/android/core/KC_f;
    .locals 6

    const/4 v5, 0x0

    invoke-static {}, Landroid/opengl/EGL14;->eglGetCurrentDisplay()Landroid/opengl/EGLDisplay;

    move-result-object v0

    const/16 v1, 0x3059

    invoke-static {v1}, Landroid/opengl/EGL14;->eglGetCurrentSurface(I)Landroid/opengl/EGLSurface;

    move-result-object v1

    const/16 v2, 0x3057

    sget-object v3, Lcom/kamcord/android/core/KC_o;->a:[I

    invoke-static {v0, v1, v2, v3, v5}, Landroid/opengl/EGL14;->eglQuerySurface(Landroid/opengl/EGLDisplay;Landroid/opengl/EGLSurface;I[II)Z

    sget-object v2, Lcom/kamcord/android/core/KC_o;->a:[I

    aget v2, v2, v5

    const/16 v3, 0x3056

    sget-object v4, Lcom/kamcord/android/core/KC_o;->a:[I

    invoke-static {v0, v1, v3, v4, v5}, Landroid/opengl/EGL14;->eglQuerySurface(Landroid/opengl/EGLDisplay;Landroid/opengl/EGLSurface;I[II)Z

    sget-object v0, Lcom/kamcord/android/core/KC_o;->a:[I

    aget v0, v0, v5

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v2, v0}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    return-object v1
.end method

.method public final b()Lcom/kamcord/android/core/KC_p;
    .locals 10

    const/16 v9, 0x3028

    const/16 v6, 0x1908

    const/16 v3, 0xa

    const/4 v2, 0x0

    invoke-static {}, Landroid/opengl/EGL14;->eglGetCurrentDisplay()Landroid/opengl/EGLDisplay;

    move-result-object v0

    const/16 v1, 0x3059

    invoke-static {v1}, Landroid/opengl/EGL14;->eglGetCurrentSurface(I)Landroid/opengl/EGLSurface;

    move-result-object v1

    sget-object v4, Lcom/kamcord/android/core/KC_o;->a:[I

    invoke-static {v0, v1, v9, v4, v2}, Landroid/opengl/EGL14;->eglQuerySurface(Landroid/opengl/EGLDisplay;Landroid/opengl/EGLSurface;I[II)Z

    sget-object v1, Lcom/kamcord/android/core/KC_o;->a:[I

    aget v7, v1, v2

    new-array v1, v3, [Landroid/opengl/EGLConfig;

    sget-object v4, Lcom/kamcord/android/core/KC_o;->a:[I

    move v5, v2

    invoke-static/range {v0 .. v5}, Landroid/opengl/EGL14;->eglGetConfigs(Landroid/opengl/EGLDisplay;[Landroid/opengl/EGLConfig;II[II)Z

    sget-object v3, Lcom/kamcord/android/core/KC_o;->a:[I

    aget v4, v3, v2

    move v3, v2

    :goto_0
    if-ge v3, v4, :cond_2

    aget-object v5, v1, v3

    sget-object v8, Lcom/kamcord/android/core/KC_o;->a:[I

    invoke-static {v0, v5, v9, v8, v2}, Landroid/opengl/EGL14;->eglGetConfigAttrib(Landroid/opengl/EGLDisplay;Landroid/opengl/EGLConfig;I[II)Z

    sget-object v8, Lcom/kamcord/android/core/KC_o;->a:[I

    aget v8, v8, v2

    if-ne v8, v7, :cond_1

    const/16 v1, 0x3025

    sget-object v3, Lcom/kamcord/android/core/KC_o;->a:[I

    invoke-static {v0, v5, v1, v3, v2}, Landroid/opengl/EGL14;->eglGetConfigAttrib(Landroid/opengl/EGLDisplay;Landroid/opengl/EGLConfig;I[II)Z

    sget-object v1, Lcom/kamcord/android/core/KC_o;->a:[I

    aget v3, v1, v2

    const/16 v1, 0x3024

    sget-object v4, Lcom/kamcord/android/core/KC_o;->a:[I

    invoke-static {v0, v5, v1, v4, v2}, Landroid/opengl/EGL14;->eglGetConfigAttrib(Landroid/opengl/EGLDisplay;Landroid/opengl/EGLConfig;I[II)Z

    sget-object v1, Lcom/kamcord/android/core/KC_o;->a:[I

    aget v1, v1, v2

    const/16 v4, 0x3026

    sget-object v7, Lcom/kamcord/android/core/KC_o;->a:[I

    invoke-static {v0, v5, v4, v7, v2}, Landroid/opengl/EGL14;->eglGetConfigAttrib(Landroid/opengl/EGLDisplay;Landroid/opengl/EGLConfig;I[II)Z

    sget-object v0, Lcom/kamcord/android/core/KC_o;->a:[I

    aget v2, v0, v2

    const/4 v0, 0x5

    if-ne v1, v0, :cond_0

    const/16 v0, 0x1907

    :goto_1
    new-instance v1, Lcom/kamcord/android/core/KC_p;

    invoke-direct {v1, v3, v0, v2}, Lcom/kamcord/android/core/KC_p;-><init>(III)V

    move-object v0, v1

    :goto_2
    return-object v0

    :cond_0
    move v0, v6

    goto :goto_1

    :cond_1
    add-int/lit8 v3, v3, 0x1

    goto :goto_0

    :cond_2
    new-instance v0, Lcom/kamcord/android/core/KC_p;

    const/16 v1, 0x18

    invoke-direct {v0, v1, v6, v2}, Lcom/kamcord/android/core/KC_p;-><init>(III)V

    goto :goto_2
.end method

.method public final c()I
    .locals 1

    invoke-static {}, Landroid/opengl/EGL14;->eglGetCurrentContext()Landroid/opengl/EGLContext;

    move-result-object v0

    invoke-virtual {v0}, Landroid/opengl/EGLContext;->hashCode()I

    move-result v0

    return v0
.end method

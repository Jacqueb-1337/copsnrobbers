.class final Lcom/kamcord/android/core/KC_z;
.super Lcom/kamcord/android/core/KC_y;


# direct methods
.method constructor <init>()V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_y;-><init>()V

    return-void
.end method


# virtual methods
.method protected final a(IILcom/kamcord/android/core/KC_p;)I
    .locals 1

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_a;->h()Z

    move-result v0

    invoke-static {p1, p2, v0}, Lcom/kamcord/android/core/KamcordNative;->newEGLImageTexture(IIZ)I

    move-result v0

    return v0
.end method

.method protected final a(I)V
    .locals 0

    invoke-static {p1}, Lcom/kamcord/android/core/KamcordNative;->deleteEGLImageTexture(I)V

    return-void
.end method

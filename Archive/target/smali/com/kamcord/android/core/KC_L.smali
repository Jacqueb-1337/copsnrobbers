.class final Lcom/kamcord/android/core/KC_L;
.super Lcom/kamcord/android/core/KC_J;


# direct methods
.method constructor <init>(Lcom/kamcord/android/core/KC_x;)V
    .locals 0

    invoke-direct {p0, p1}, Lcom/kamcord/android/core/KC_J;-><init>(Lcom/kamcord/android/core/KC_x;)V

    return-void
.end method


# virtual methods
.method public final a(Lcom/kamcord/android/core/KC_y;J)Z
    .locals 4

    const/4 v2, 0x1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_L;->b:Lcom/kamcord/android/core/KC_x;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_L;->c:Lcom/kamcord/android/core/KC_z;

    invoke-virtual {v0, v2, v2, p1, v1}, Lcom/kamcord/android/core/KC_x;->a(ZZLcom/kamcord/android/core/KC_y;Lcom/kamcord/android/core/KC_y;)V

    invoke-static {}, Landroid/opengl/GLES20;->glFinish()V

    new-instance v0, Lcom/kamcord/android/core/KC_F;

    invoke-direct {v0, p2, p3}, Lcom/kamcord/android/core/KC_F;-><init>(J)V

    invoke-virtual {p0, v0}, Lcom/kamcord/android/core/KC_L;->a(Lcom/kamcord/android/core/KC_F;)V

    return v2
.end method

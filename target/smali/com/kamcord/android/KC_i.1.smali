.class public final Lcom/kamcord/android/KC_i;
.super La/a/a/a/KC_e;


# instance fields
.field M:Lcom/kamcord/android/KamcordActivity;

.field private N:Landroid/view/View;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, La/a/a/a/KC_e;-><init>()V

    return-void
.end method


# virtual methods
.method public final a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
    .locals 3

    const-string v0, "layout"

    const-string v1, "z_kamcord_fragment_edit_profile"

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    const/4 v1, 0x0

    invoke-virtual {p1, v0, p2, v1}, Landroid/view/LayoutInflater;->inflate(ILandroid/view/ViewGroup;Z)Landroid/view/View;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_i;->N:Landroid/view/View;

    iget-object v0, p0, Lcom/kamcord/android/KC_i;->N:Landroid/view/View;

    iput-object v0, p0, Lcom/kamcord/android/KC_i;->N:Landroid/view/View;

    iget-object v0, p0, Lcom/kamcord/android/KC_i;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "changeEmailLayout"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/LinearLayout;

    new-instance v1, Lcom/kamcord/android/KC_i$1;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_i$1;-><init>(Lcom/kamcord/android/KC_i;)V

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    new-instance v1, Lcom/kamcord/android/c/KC_a;

    invoke-direct {v1}, Lcom/kamcord/android/c/KC_a;-><init>()V

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setOnTouchListener(Landroid/view/View$OnTouchListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_i;->N:Landroid/view/View;

    const-string v1, "id"

    const-string v2, "changePasswordLayout"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/view/View;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/widget/LinearLayout;

    new-instance v1, Lcom/kamcord/android/KC_i$2;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_i$2;-><init>(Lcom/kamcord/android/KC_i;)V

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    new-instance v1, Lcom/kamcord/android/c/KC_a;

    invoke-direct {v1}, Lcom/kamcord/android/c/KC_a;-><init>()V

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setOnTouchListener(Landroid/view/View$OnTouchListener;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_i;->N:Landroid/view/View;

    return-object v0
.end method

.method public final a(Landroid/app/Activity;)V
    .locals 0

    invoke-super {p0, p1}, La/a/a/a/KC_e;->a(Landroid/app/Activity;)V

    check-cast p1, Lcom/kamcord/android/KamcordActivity;

    iput-object p1, p0, Lcom/kamcord/android/KC_i;->M:Lcom/kamcord/android/KamcordActivity;

    return-void
.end method

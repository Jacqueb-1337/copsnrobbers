.class final La/a/a/a/KC_l;
.super Ljava/lang/Object;
.source "SourceFile"

# interfaces
.implements Landroid/os/Parcelable;


# static fields
.field public static final CREATOR:Landroid/os/Parcelable$Creator;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Landroid/os/Parcelable$Creator",
            "<",
            "La/a/a/a/KC_l;",
            ">;"
        }
    .end annotation
.end field


# instance fields
.field a:Landroid/os/Bundle;

.field b:La/a/a/a/KC_e;

.field private c:Ljava/lang/String;

.field private d:I

.field private e:Z

.field private f:I

.field private g:I

.field private h:Ljava/lang/String;

.field private i:Z

.field private j:Z

.field private k:Landroid/os/Bundle;


# direct methods
.method static constructor <clinit>()V
    .locals 1

    .prologue
    .line 138
    new-instance v0, La/a/a/a/KC_l$1;

    invoke-direct {v0}, La/a/a/a/KC_l$1;-><init>()V

    sput-object v0, La/a/a/a/KC_l;->CREATOR:Landroid/os/Parcelable$Creator;

    return-void
.end method

.method public constructor <init>(La/a/a/a/KC_e;)V
    .locals 1

    .prologue
    .line 65
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 66
    invoke-virtual {p1}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/Class;->getName()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, La/a/a/a/KC_l;->c:Ljava/lang/String;

    .line 67
    iget v0, p1, La/a/a/a/KC_e;->f:I

    iput v0, p0, La/a/a/a/KC_l;->d:I

    .line 68
    iget-boolean v0, p1, La/a/a/a/KC_e;->o:Z

    iput-boolean v0, p0, La/a/a/a/KC_l;->e:Z

    .line 69
    iget v0, p1, La/a/a/a/KC_e;->w:I

    iput v0, p0, La/a/a/a/KC_l;->f:I

    .line 70
    iget v0, p1, La/a/a/a/KC_e;->x:I

    iput v0, p0, La/a/a/a/KC_l;->g:I

    .line 71
    iget-object v0, p1, La/a/a/a/KC_e;->y:Ljava/lang/String;

    iput-object v0, p0, La/a/a/a/KC_l;->h:Ljava/lang/String;

    .line 72
    iget-boolean v0, p1, La/a/a/a/KC_e;->B:Z

    iput-boolean v0, p0, La/a/a/a/KC_l;->i:Z

    .line 73
    iget-boolean v0, p1, La/a/a/a/KC_e;->A:Z

    iput-boolean v0, p0, La/a/a/a/KC_l;->j:Z

    .line 74
    iget-object v0, p1, La/a/a/a/KC_e;->h:Landroid/os/Bundle;

    iput-object v0, p0, La/a/a/a/KC_l;->k:Landroid/os/Bundle;

    .line 75
    return-void
.end method

.method public constructor <init>(Landroid/os/Parcel;)V
    .locals 3

    .prologue
    const/4 v1, 0x1

    const/4 v2, 0x0

    .line 77
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 78
    invoke-virtual {p1}, Landroid/os/Parcel;->readString()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, La/a/a/a/KC_l;->c:Ljava/lang/String;

    .line 79
    invoke-virtual {p1}, Landroid/os/Parcel;->readInt()I

    move-result v0

    iput v0, p0, La/a/a/a/KC_l;->d:I

    .line 80
    invoke-virtual {p1}, Landroid/os/Parcel;->readInt()I

    move-result v0

    if-eqz v0, :cond_0

    move v0, v1

    :goto_0
    iput-boolean v0, p0, La/a/a/a/KC_l;->e:Z

    .line 81
    invoke-virtual {p1}, Landroid/os/Parcel;->readInt()I

    move-result v0

    iput v0, p0, La/a/a/a/KC_l;->f:I

    .line 82
    invoke-virtual {p1}, Landroid/os/Parcel;->readInt()I

    move-result v0

    iput v0, p0, La/a/a/a/KC_l;->g:I

    .line 83
    invoke-virtual {p1}, Landroid/os/Parcel;->readString()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, La/a/a/a/KC_l;->h:Ljava/lang/String;

    .line 84
    invoke-virtual {p1}, Landroid/os/Parcel;->readInt()I

    move-result v0

    if-eqz v0, :cond_1

    move v0, v1

    :goto_1
    iput-boolean v0, p0, La/a/a/a/KC_l;->i:Z

    .line 85
    invoke-virtual {p1}, Landroid/os/Parcel;->readInt()I

    move-result v0

    if-eqz v0, :cond_2

    :goto_2
    iput-boolean v1, p0, La/a/a/a/KC_l;->j:Z

    .line 86
    invoke-virtual {p1}, Landroid/os/Parcel;->readBundle()Landroid/os/Bundle;

    move-result-object v0

    iput-object v0, p0, La/a/a/a/KC_l;->k:Landroid/os/Bundle;

    .line 87
    invoke-virtual {p1}, Landroid/os/Parcel;->readBundle()Landroid/os/Bundle;

    move-result-object v0

    iput-object v0, p0, La/a/a/a/KC_l;->a:Landroid/os/Bundle;

    .line 88
    return-void

    :cond_0
    move v0, v2

    .line 80
    goto :goto_0

    :cond_1
    move v0, v2

    .line 84
    goto :goto_1

    :cond_2
    move v1, v2

    .line 85
    goto :goto_2
.end method


# virtual methods
.method public final a(La/a/a/a/KC_f;La/a/a/a/KC_e;)La/a/a/a/KC_e;
    .locals 2

    .prologue
    .line 91
    iget-object v0, p0, La/a/a/a/KC_l;->b:La/a/a/a/KC_e;

    if-eqz v0, :cond_0

    .line 92
    iget-object v0, p0, La/a/a/a/KC_l;->b:La/a/a/a/KC_e;

    .line 115
    :goto_0
    return-object v0

    .line 95
    :cond_0
    iget-object v0, p0, La/a/a/a/KC_l;->k:Landroid/os/Bundle;

    if-eqz v0, :cond_1

    .line 96
    iget-object v0, p0, La/a/a/a/KC_l;->k:Landroid/os/Bundle;

    invoke-virtual {p1}, La/a/a/a/KC_f;->getClassLoader()Ljava/lang/ClassLoader;

    move-result-object v1

    invoke-virtual {v0, v1}, Landroid/os/Bundle;->setClassLoader(Ljava/lang/ClassLoader;)V

    .line 99
    :cond_1
    iget-object v0, p0, La/a/a/a/KC_l;->c:Ljava/lang/String;

    iget-object v1, p0, La/a/a/a/KC_l;->k:Landroid/os/Bundle;

    invoke-static {p1, v0, v1}, La/a/a/a/KC_e;->a(Landroid/content/Context;Ljava/lang/String;Landroid/os/Bundle;)La/a/a/a/KC_e;

    move-result-object v0

    iput-object v0, p0, La/a/a/a/KC_l;->b:La/a/a/a/KC_e;

    .line 101
    iget-object v0, p0, La/a/a/a/KC_l;->a:Landroid/os/Bundle;

    if-eqz v0, :cond_2

    .line 102
    iget-object v0, p0, La/a/a/a/KC_l;->a:Landroid/os/Bundle;

    invoke-virtual {p1}, La/a/a/a/KC_f;->getClassLoader()Ljava/lang/ClassLoader;

    move-result-object v1

    invoke-virtual {v0, v1}, Landroid/os/Bundle;->setClassLoader(Ljava/lang/ClassLoader;)V

    .line 103
    iget-object v0, p0, La/a/a/a/KC_l;->b:La/a/a/a/KC_e;

    iget-object v1, p0, La/a/a/a/KC_l;->a:Landroid/os/Bundle;

    iput-object v1, v0, La/a/a/a/KC_e;->d:Landroid/os/Bundle;

    .line 105
    :cond_2
    iget-object v0, p0, La/a/a/a/KC_l;->b:La/a/a/a/KC_e;

    iget v1, p0, La/a/a/a/KC_l;->d:I

    invoke-virtual {v0, v1, p2}, La/a/a/a/KC_e;->a(ILa/a/a/a/KC_e;)V

    .line 106
    iget-object v0, p0, La/a/a/a/KC_l;->b:La/a/a/a/KC_e;

    iget-boolean v1, p0, La/a/a/a/KC_l;->e:Z

    iput-boolean v1, v0, La/a/a/a/KC_e;->o:Z

    .line 107
    iget-object v0, p0, La/a/a/a/KC_l;->b:La/a/a/a/KC_e;

    const/4 v1, 0x1

    iput-boolean v1, v0, La/a/a/a/KC_e;->q:Z

    .line 108
    iget-object v0, p0, La/a/a/a/KC_l;->b:La/a/a/a/KC_e;

    iget v1, p0, La/a/a/a/KC_l;->f:I

    iput v1, v0, La/a/a/a/KC_e;->w:I

    .line 109
    iget-object v0, p0, La/a/a/a/KC_l;->b:La/a/a/a/KC_e;

    iget v1, p0, La/a/a/a/KC_l;->g:I

    iput v1, v0, La/a/a/a/KC_e;->x:I

    .line 110
    iget-object v0, p0, La/a/a/a/KC_l;->b:La/a/a/a/KC_e;

    iget-object v1, p0, La/a/a/a/KC_l;->h:Ljava/lang/String;

    iput-object v1, v0, La/a/a/a/KC_e;->y:Ljava/lang/String;

    .line 111
    iget-object v0, p0, La/a/a/a/KC_l;->b:La/a/a/a/KC_e;

    iget-boolean v1, p0, La/a/a/a/KC_l;->i:Z

    iput-boolean v1, v0, La/a/a/a/KC_e;->B:Z

    .line 112
    iget-object v0, p0, La/a/a/a/KC_l;->b:La/a/a/a/KC_e;

    iget-boolean v1, p0, La/a/a/a/KC_l;->j:Z

    iput-boolean v1, v0, La/a/a/a/KC_e;->A:Z

    .line 113
    iget-object v0, p0, La/a/a/a/KC_l;->b:La/a/a/a/KC_e;

    iget-object v1, p1, La/a/a/a/KC_f;->b:La/a/a/a/KC_i;

    iput-object v1, v0, La/a/a/a/KC_e;->s:La/a/a/a/KC_i;

    .line 115
    iget-object v0, p0, La/a/a/a/KC_l;->b:La/a/a/a/KC_e;

    goto :goto_0
.end method

.method public final describeContents()I
    .locals 1

    .prologue
    .line 122
    const/4 v0, 0x0

    return v0
.end method

.method public final writeToParcel(Landroid/os/Parcel;I)V
    .locals 3
    .param p1, "dest"    # Landroid/os/Parcel;
    .param p2, "flags"    # I

    .prologue
    const/4 v1, 0x1

    const/4 v2, 0x0

    .line 126
    iget-object v0, p0, La/a/a/a/KC_l;->c:Ljava/lang/String;

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeString(Ljava/lang/String;)V

    .line 127
    iget v0, p0, La/a/a/a/KC_l;->d:I

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeInt(I)V

    .line 128
    iget-boolean v0, p0, La/a/a/a/KC_l;->e:Z

    if-eqz v0, :cond_0

    move v0, v1

    :goto_0
    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeInt(I)V

    .line 129
    iget v0, p0, La/a/a/a/KC_l;->f:I

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeInt(I)V

    .line 130
    iget v0, p0, La/a/a/a/KC_l;->g:I

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeInt(I)V

    .line 131
    iget-object v0, p0, La/a/a/a/KC_l;->h:Ljava/lang/String;

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeString(Ljava/lang/String;)V

    .line 132
    iget-boolean v0, p0, La/a/a/a/KC_l;->i:Z

    if-eqz v0, :cond_1

    move v0, v1

    :goto_1
    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeInt(I)V

    .line 133
    iget-boolean v0, p0, La/a/a/a/KC_l;->j:Z

    if-eqz v0, :cond_2

    :goto_2
    invoke-virtual {p1, v1}, Landroid/os/Parcel;->writeInt(I)V

    .line 134
    iget-object v0, p0, La/a/a/a/KC_l;->k:Landroid/os/Bundle;

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeBundle(Landroid/os/Bundle;)V

    .line 135
    iget-object v0, p0, La/a/a/a/KC_l;->a:Landroid/os/Bundle;

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeBundle(Landroid/os/Bundle;)V

    .line 136
    return-void

    :cond_0
    move v0, v2

    .line 128
    goto :goto_0

    :cond_1
    move v0, v2

    .line 132
    goto :goto_1

    :cond_2
    move v1, v2

    .line 133
    goto :goto_2
.end method
